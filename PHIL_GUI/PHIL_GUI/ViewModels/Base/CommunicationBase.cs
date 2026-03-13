using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using PHIL_GUI.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PHIL_GUI.ViewModels.Base
{
    enum LimitType
    {
        Released,
        Pressed
    }
    public abstract class CommunicationBase : ViewModelBase
    {
        const string WELL_PREFIX = "WELL:";
        const string POS_PREFIX = "POS:";
        const string CAL_COUNT_PREFIX = "CAL_COUNT:";
        const string LIMIT_PRESSED_PREFIX = "LIMIT_PRESSED:";
        const string LIMIT_RELEASED_PREFIX = "LIMIT_RELEASED:";

        protected readonly SerialPortService SerialService;
        protected readonly RobotStateService RobotState;

        private string _receivedData = "";
        public string ReceivedData
        {
            get => _receivedData;
            private set => SetProperty(ref _receivedData, value);
        }

        protected CommunicationBase()   
        {
            SerialService = App.Services.GetRequiredService<SerialPortService>();
            RobotState = App.Services.GetRequiredService<RobotStateService>();
            SerialService.MessageReceived += OnMessageReceived;
        }

        private void OnMessageReceived(string message)
        {
            Dispatcher.UIThread.Post(() =>
            {
                ReceivedData += $"{DateTime.Now:HH:mm:ss}: {message}\n";
            });

            //AppendLog();

            if (message.StartsWith(WELL_PREFIX)) ParseWellArrival(message);
            else if (message.StartsWith(POS_PREFIX)) ParsePosition(message);
            else if (message.StartsWith(CAL_COUNT_PREFIX)) ParseCalCount(message);
            else if (message.StartsWith(LIMIT_PRESSED_PREFIX)) ParseLimit(message, LimitType.Pressed);
            else if (message.StartsWith(LIMIT_RELEASED_PREFIX)) ParseLimit(message, LimitType.Released);
            //else if (message.StartsWith("CAL_PT:")) ParseCalPoint(message);
            //else if (message.StartsWith("CAL_REC:")) ParseCalRecorded(message);
            //else if (message.StartsWith("CAL_ERR:")) ParseCalError(message);
            //else if (message.StartsWith("CAL_COEFFS_L:")) ParseCoeffsL(message);
            //else if (message.StartsWith("CAL_COEFFS_R:")) ParseCoeffsR(message);
            //else if (message.StartsWith("RMS:")) ParseRms(message);
            //else if (message.StartsWith("LIMIT:")) ParseLimit(message);
            //else if (message.StartsWith("ERROR:"))   ParseAlert(message, AlertLevel.Error);
        }

        protected void Send(string command)
        {
            SerialService.SendMessage(command);
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

            RobotState.CurrentWell.Type = Models.WellType.Standard;
            RobotState.CurrentWell.Name = parts[0].ToUpper();
            RobotState.CurrentWell.X = kv["X"];
            RobotState.CurrentWell.Y = kv["Y"];
            RobotState.CurrentWell.AngleL = kv["L"];
            RobotState.CurrentWell.AngleR = kv["R"].Trim();

            RobotState.State = MoveState.Idle;
        }

        private void ParsePosition(string msg)
        {
            var d = ParseKV(msg, POS_PREFIX);

            RobotState.Position.L = d["L"];
            RobotState.Position.R = d["R"];
            RobotState.Position.Z1 = d["Z1"];
            RobotState.Position.Z2 = d["Z2"].Trim();

            if (RobotState.State == MoveState.EmergencyStopped) return;

            // If we're getting position updates, we must be moving (unless we e-stopped)
            RobotState.State = MoveState.Idle;
        }

        private void ParseCalCount(string msg)
        {
            var parts = msg.Substring(CAL_COUNT_PREFIX.Length);

            if (!int.TryParse(parts, out int count)) return;

            RobotState.Calibration.Count = count;
        }

        private void ParseLimit(string msg, LimitType type)
        {
            string prefix = type == LimitType.Pressed ? LIMIT_PRESSED_PREFIX : LIMIT_RELEASED_PREFIX;
            bool state = type == LimitType.Pressed;

            var d = ParseKV(msg, prefix);
            var axis = d["AXIS"].Trim();

            if (axis == "Z1") RobotState.Limit.Z1 = state;
            else if (axis == "Z2") RobotState.Limit.Z2 = state;
            else if (axis == "L") RobotState.Limit.L = state;
            else if (axis == "R") RobotState.Limit.R = state;
        }

        //private void ParseRms(string msg)
        //{
        //    var d = ParseKV(msg, "RMS:");
        //    Dispatcher.UIThread.Post(() => {
        //        RmsL = double.Parse(d["L"], CultureInfo.InvariantCulture);
        //        RmsR = double.Parse(d["R"], CultureInfo.InvariantCulture);
        //        MaxErrL = double.Parse(d["MAX_L"], CultureInfo.InvariantCulture);
        //        MaxErrR = double.Parse(d["MAX_R"], CultureInfo.InvariantCulture);
        //    });
        //}

        //private void ParseCalPoint(string msg)
        //{
        //    // "CAL_PT:0,0.00,0.00,22.27,-74.02"
        //    var parts = msg.Substring("CAL_PT:".Length).Split(',');
        //    Dispatcher.UIThread.Post(() => {
        //        CalibrationPoints.Add(new CalibrationPoint
        //        {
        //            Index = int.Parse(parts[0]),
        //            X = double.Parse(parts[1], CultureInfo.InvariantCulture),
        //            Y = double.Parse(parts[2], CultureInfo.InvariantCulture),
        //            AngleL = double.Parse(parts[3], CultureInfo.InvariantCulture),
        //            AngleR = double.Parse(parts[4], CultureInfo.InvariantCulture),
        //        });
        //    });
        //}

        //private void ParseCalError(string msg)
        //{
        //    // "CAL_ERR:0,0.00,0.00,0.499,-0.296"
        //    var parts = msg.Substring("CAL_ERR:".Length).Split(',');
        //    int idx = int.Parse(parts[0]);
        //    var pt = CalibrationPoints.FirstOrDefault(p => p.Index == idx);
        //    if (pt != null) Dispatcher.UIThread.Post(() => {
        //        pt.ErrL = double.Parse(parts[3], CultureInfo.InvariantCulture);
        //        pt.ErrR = double.Parse(parts[4], CultureInfo.InvariantCulture);
        //    });
        //}


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
