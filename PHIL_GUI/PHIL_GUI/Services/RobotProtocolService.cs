using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using PHIL_GUI.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PHIL_GUI.Services
{
    enum LimitType
    {
        Released,
        Pressed
    }
    public class RobotProtocolService : ObservableObject
    {
        const string WELL_PREFIX = "WELL:";
        const string POS_PREFIX = "POS:";
        const string CAL_PT_PREFIX = "CAL_PT:";
        const string CAL_REC_PREFIX = "CAL_REC:";
        const string RMS_PREFIX = "RMS:";
        const string LIMIT_PRESSED_PREFIX = "LIMIT_PRESSED:";
        const string LIMIT_RELEASED_PREFIX = "LIMIT_RELEASED:";
        const string STEP_SIZE_PREFIX = "STEP_SIZE:";


        private readonly SerialPortService serialPort;
        public SerialPortService SerialPort => serialPort;
        private readonly RobotState robotState;
        public RobotState RobotState => robotState;

        private string _receivedData = "";
        public string ReceivedData
        {
            get => _receivedData;
            private set => SetProperty(ref _receivedData, value);
        }

        public RobotProtocolService(SerialPortService serial)
        {
            serialPort = serial;
            robotState = new RobotState();
            serial.MessageReceived += OnMessageReceived;
        }

        public void Send(string command)
        {
            serialPort.SendMessage(command);
        }

        public void MoveUp()
        {
            Send("u");
        }

        public void MoveDown()
        {
            Send("d");
        }

        public void MoveForward()
        {
            Send("f");
        }

        public void MoveBackward()
        {
            Send("b");
        }

        public void MoveLeft()
        {
            Send("l");
        }

        public void MoveRight()
        {
            Send("r");
        }

        public void GoHome()
        {
            robotState.CurrentWell.Type = WellType.Home;
            robotState.Settings.State = MoveState.Moving;
            Send("h");
        }

        public void CalibrateHome()
        {
            robotState.CurrentWell.Type = WellType.Home;
            robotState.Settings.State = MoveState.Moving;
            Send("c");
        }

        public void EmergencyStop()
        {
            robotState.CurrentWell.Type = WellType.Unknown;
            robotState.Settings.State = MoveState.EmergencyStopped;
            Send("s");
        }

        public void GetSetupInformation()
        {
            // Current Well
            Send("pw");
            // Curent Calibration Points
            Send("pm");
            // Current Step Size
            Send("ps");
        }

        private void OnMessageReceived(string message)
        {
            Dispatcher.UIThread.Post(() =>
            {
                ReceivedData += $"{DateTime.Now:HH:mm:ss}: {message}\n";
                if (message.StartsWith(CAL_REC_PREFIX)) ParseCalRecorded(message);
                else if (message.StartsWith(CAL_PT_PREFIX)) ParseCalPoint(message);
            });


            if (message.StartsWith(WELL_PREFIX)) ParseWellArrival(message);
            else if (message.StartsWith(POS_PREFIX)) ParsePosition(message);
            else if (message.StartsWith(RMS_PREFIX)) ParseRms(message);
            else if (message.StartsWith(LIMIT_PRESSED_PREFIX)) ParseLimit(message, LimitType.Pressed);
            else if (message.StartsWith(LIMIT_RELEASED_PREFIX)) ParseLimit(message, LimitType.Released);
            else if (message.StartsWith(STEP_SIZE_PREFIX)) ParseStepSize(message);
            //else if (message.StartsWith("CAL_COEFFS_L:")) ParseCoeffsL(message);
            //else if (message.StartsWith("CAL_COEFFS_R:")) ParseCoeffsR(message);
            //else if (message.StartsWith("LIMIT:")) ParseLimit(message);
            //else if (message.StartsWith("ERROR:"))   ParseAlert(message, AlertLevel.Error);
        }

        private Dictionary<string, string> ParseKV(string msg, string prefix)
        {
            return msg.Substring(prefix.Length)
                      .Split(',')
                      .Select(p => p.Split('='))
                      .Where(p => p.Length == 2)
                      .ToDictionary(p => p[0], p => p[1]);
        }

        private void ParseWellArrival(string msg)
        {
            string content = msg.Substring(WELL_PREFIX.Length);
            string[] parts = content.Split(',');

            var kv = parts.Skip(1)
              .Select(p => p.Split('='))
              .Where(p => p.Length == 2)
              .ToDictionary(p => p[0], p => p[1]);

            robotState.CurrentWell.Type = WellType.Standard;
            robotState.CurrentWell.Name = parts[0].ToUpper();
            robotState.CurrentWell.X = kv["X"];
            robotState.CurrentWell.Y = kv["Y"];
            robotState.CurrentWell.AngleL = kv["L"];
            robotState.CurrentWell.AngleR = kv["R"].Trim();

            robotState.Settings.State = MoveState.Idle;
        }

        private void ParsePosition(string msg)
        {
            var d = ParseKV(msg, POS_PREFIX);

            robotState.Position.L = d["L"];
            robotState.Position.R = d["R"];
            robotState.Position.Z1 = d["Z1"];
            robotState.Position.Z2 = d["Z2"].Trim();

            if (robotState.Settings.State == MoveState.EmergencyStopped) return;

            // If we're getting position updates, we must be moving (unless we e-stopped)
            robotState.Settings.State = MoveState.Idle;
        }

        private void ParseCalPoint(string msg)
        {
            var parts = msg.Substring(CAL_PT_PREFIX.Length).Split(',');
            {
                string name = parts[0].ToUpper();
                CalibrationPoint existingPoint = robotState.Calibration.Points.FirstOrDefault(p => p.Name == name);
                if (existingPoint != null)
                {
                    existingPoint.X = (int)double.Parse(parts[1], CultureInfo.InvariantCulture);
                    existingPoint.Y = (int)double.Parse(parts[2].Trim(), CultureInfo.InvariantCulture);
                    existingPoint.ErrorLeft = double.Parse(parts[3], CultureInfo.InvariantCulture);
                    existingPoint.ErrorRight = double.Parse(parts[4].Trim(), CultureInfo.InvariantCulture);
                }
                else
                    robotState.Calibration.Points.Add(
                    new CalibrationPoint
                    {
                        Name = parts[0].ToUpper(),
                        X = (int)double.Parse(parts[1], CultureInfo.InvariantCulture),
                        Y = (int)double.Parse(parts[2].Trim(), CultureInfo.InvariantCulture),
                        ErrorLeft = parts.Length > 3 ? double.Parse(parts[3], CultureInfo.InvariantCulture) : -2,
                        ErrorRight = parts.Length > 3 ? double.Parse(parts[4].Trim(), CultureInfo.InvariantCulture) : -2,
                    });
            };
        }

        private void ParseCalRecorded(string msg)
        {
            var parts = msg.Substring(CAL_REC_PREFIX.Length).Split(',');
            {
                robotState.Calibration.Points.Add(
                    new CalibrationPoint
                    {
                        Name = parts[0].ToUpper(),
                        X = (int)double.Parse(parts[1], CultureInfo.InvariantCulture),
                        Y = (int)double.Parse(parts[2].Trim(), CultureInfo.InvariantCulture),
                        ErrorLeft = -2,
                        ErrorRight = -2,
                    });
            };
        }

        private void ParseRms(string msg)
        {
            var d = ParseKV(msg, RMS_PREFIX);
            robotState.Calibration.RmsLValue = double.Parse(d["L"], CultureInfo.InvariantCulture);
            robotState.Calibration.RmsRValue = double.Parse(d["R"], CultureInfo.InvariantCulture);
        }

        private void ParseLimit(string msg, LimitType type)
        {
            string prefix = type == LimitType.Pressed ? LIMIT_PRESSED_PREFIX : LIMIT_RELEASED_PREFIX;
            bool state = type == LimitType.Pressed;

            var d = ParseKV(msg, prefix);
            var axis = d["AXIS"].Trim();

            if (axis == "Z1") robotState.Limit.Z1 = state;
            else if (axis == "Z2") robotState.Limit.Z2 = state;
            else if (axis == "L") robotState.Limit.L = state;
            else if (axis == "R") robotState.Limit.R = state;
        }

        private void ParseStepSize(string msg)
        {
            string d = msg.Substring(STEP_SIZE_PREFIX.Length).Trim();
            robotState.Settings.StepSize = double.Parse(d, CultureInfo.InvariantCulture);
        }


        //private void ParseAlert(string msg, AlertLevel level)
        //{
        //    // "WARNING:LIMIT_HIT,Z1=1,Z2=0"
        //    // "ERROR:HOME_FAILED,Homing timed out..."
        //    var body = msg.Substring(msg.IndexOf(':') + 1);
        //    var comma = body.IndexOf(',');

        //    var code = comma >= 0 ? body.Substring(0, comma) : body;
        //    var detail = comma >= 0 ? body.Substring(comma + 1) : "";

        //    Dispatcher.UIThread.Post(() => {
        //        Alerts.Insert(0, new RobotAlert
        //        {
        //            Level = level,
        //            Code = code,
        //            Message = detail
        //        });

        //        // Cap history
        //        while (Alerts.Count > 50) Alerts.RemoveAt(Alerts.Count - 1);

        //        // Surface the most recent alert for status bar binding
        //        LatestAlert = Alerts[0];
        //    });

        //public enum AlertLevel { Info, Warning, Error }

        //public class RobotAlert
        //{
        //    public AlertLevel Level { get; set; }
        //    public string Code { get; set; }  // e.g. "LIMIT_HIT"
        //    public string Message { get; set; }
        //    public DateTime Time { get; set; } = DateTime.Now;
        //}

    }
}
