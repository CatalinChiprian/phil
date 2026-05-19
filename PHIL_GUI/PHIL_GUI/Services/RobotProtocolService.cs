using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using PHIL_GUI.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace PHIL_GUI.Services
{
    enum LimitType
    {
        Released,
        Pressed
    }
    public class RobotProtocolService : ObservableObject
    {
        const string MOVE_BACKWARD_CMD = "MOVE_BACKWARD";
        const string MOVE_FORWARD_CMD = "MOVE_FORWARD";
        const string MOVE_LEFT_CMD = "MOVE_LEFT";
        const string MOVE_RIGHT_CMD = "MOVE_RIGHT";
        const string MOVE_UP_CMD = "MOVE_UP";
        const string MOVE_DOWN_CMD = "MOVE_DOWN";
        const string GO_HOME_CMD = "GO_HOME";
        const string INC_STEP_CMD = "INC_STEP";
        const string DEC_STEP_CMD = "DEC_STEP";
        const string ASPIRATE_CMD = "ASPIRATE";
        const string DISPENSE_CMD = "DISPENSE";
        const string CALIBRATE_HOME_CMD = "CALIBRATE_HOME";
        const string MOVE_HARD_WELL_CMD = "MOVE_HARD_WELL";
        const string MOVE_CALC_WELL_CMD = "MOVE_CALC_WELL";
        const string RECORD_POINT_CMD = "RECORD_POINT";
        const string SOLVE_MAP_CMD = "SOLVE_MAP";
        const string DELETE_POINT_CMD = "DELETE_POINT";
        const string CLEAR_CALIBRATION_CMD = "CLEAR_CALIBRATION";
        const string PARK_CMD = "PARK";
        const string PRINT_WELL_CMD = "PRINT_WELL";
        const string PRINT_CALIBRATION_CMD = "PRINT_CALIBRATION";
        const string PRINT_STEPS_CMD = "PRINT_STEPS";
        const string CREATE_ACTION_CMD = "CREATE_ACTION";
        const string UPDATE_ACTION_CMD = "UPDATE_ACTION";
        const string DEL_ACTION_CMD = "DEL_ACTION";
        const string LINK_ACTION_WELL_CMD = "LINK_ACTION_WELL";
        const string UNLINK_ACTION_WELL_CMD = "UNLINK_ACTION_WELL";

        const string WELL_PREFIX = "WELL:";
        const string POS_PREFIX = "POS:";
        const string CAL_PT_PREFIX = "CAL_PT:";
        const string CAL_REC_PREFIX = "CAL_REC:";
        const string ACTION_PREFIX = "ACTION:";
        const string WELL_ACTION_PREFIX = "WELL_ACTION:";
        const string RMS_PREFIX = "RMS:";
        const string LIMIT_PRESSED_PREFIX = "LIMIT_PRESSED:";
        const string LIMIT_RELEASED_PREFIX = "LIMIT_RELEASED:";
        const string STEP_SIZE_PREFIX = "STEP_SIZE:";
        const string MICROSTEPS_PREFIX = "MICROSTEPS:";

        private bool ready;

        private readonly SerialPortService serialPort;
        public SerialPortService SerialPort => serialPort;
        private readonly RobotState robotState;
        public RobotState RobotState => robotState;

        private readonly StringBuilder logBuffer = new();
        private readonly DispatcherTimer logTimer;

        private string _receivedData = "";
        public string ReceivedData
        {
            get => _receivedData;
            private set => SetProperty(ref _receivedData, value);
        }

        public RobotProtocolService(SerialPortService serial)
        {
            logTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            logTimer.Tick += (_, _) =>
            {
                if (logBuffer.Length > 0)
                {
                    ReceivedData += logBuffer.ToString();
                    logBuffer.Clear();
                }
            };
            logTimer.Start();


            serialPort = serial;
            robotState = new RobotState();
            serial.MessageReceived += OnMessageReceived;
            serial.GetStartUpMessage += GetSetupInformation;
        }

        private void Send(string command)
        {
            serialPort.SendMessage(command);
        }

        public void ClearReceivedData()
        {
            ReceivedData = "";
        }

        public void Stop()
        {
            Send("s");
        }

        public void MoveUp()
        {
            Send(MOVE_UP_CMD);
        }

        public void MoveDown()
        {
            Send(MOVE_DOWN_CMD);
        }

        public void MoveForward()
        {
            Send(MOVE_FORWARD_CMD);
        }

        public void MoveBackward()
        {
            Send(MOVE_BACKWARD_CMD);
        }

        public void MoveLeft()
        {
            Send(MOVE_LEFT_CMD);
        }

        public void MoveRight()
        {
            Send(MOVE_RIGHT_CMD);
        }

        public void DecreaseStepSize()
        {
            Send(DEC_STEP_CMD);
        }

        public void IncreaseStepSize()
        {
            Send(INC_STEP_CMD);
        }

        public void RecordCalibrationPoint(string wellName)
        {
            Send($"{RECORD_POINT_CMD} {wellName}");
        }

        public void SolveMap()
        {
            Send(SOLVE_MAP_CMD);
        }

        public void DeleteCalibrationPoint(string wellName)
        {
            Send($"{DELETE_POINT_CMD} {wellName}");
        }

        public void Aspirate(int pumpNumber, int volume)
        {
            Send($"{ASPIRATE_CMD} {pumpNumber} {volume}");
        }

        public void Dispense(int pumpNumber, int volume)
        {
            Send($"{DISPENSE_CMD} {pumpNumber} {volume}");
        }

        public void Prime(int pumpNumber)
        {
            Send($"{DISPENSE_CMD} {pumpNumber} {int.MaxValue}");
        }

        public void MoveToHardcodedWell(string wellName)
        {
            Send($"{MOVE_HARD_WELL_CMD} {wellName}");
        }

        public void MoveToCalculatedWell(string wellName)
        {
            Send($"{MOVE_CALC_WELL_CMD} {wellName}");
        }

        public void GoHome()
        {
            robotState.CurrentWell.Type = WellType.Home;
            robotState.Settings.State = MoveState.Moving;
            Send(GO_HOME_CMD);
        }

        public void CalibrateHome()
        {
            robotState.CurrentWell.Type = WellType.Home;
            robotState.Settings.State = MoveState.Moving;
            Send(CALIBRATE_HOME_CMD);
        }

        public void EmergencyStop()
        {
            robotState.CurrentWell.Type = WellType.Unknown;
            robotState.Settings.State = MoveState.EmergencyStopped;
            Send("s");
        }

        public void GetSetupInformation()
        {
            ready = true;

            Send(PRINT_WELL_CMD);
            Send(PRINT_CALIBRATION_CMD);
            Send(PRINT_STEPS_CMD);
        }

        private void OnMessageReceived(string message)
        {
            if (!ready) return;

            logBuffer.AppendLine($"{DateTime.Now:HH:mm:ss}: {message}");

            Dispatcher.UIThread.Post(() =>
            {
                ApplyMessage(message);
            }, DispatcherPriority.Background);
        }

        private void ApplyMessage(string message)
        {
            if (message.StartsWith(WELL_PREFIX)) ParseWellArrival(message);
            else if (message.StartsWith(CAL_REC_PREFIX)) ParseCalRecorded(message);
            else if (message.StartsWith(CAL_PT_PREFIX)) ParseCalPoint(message);
            else if (message.StartsWith(POS_PREFIX)) ParsePosition(message);
            else if (message.StartsWith(RMS_PREFIX)) ParseRms(message);
            else if (message.StartsWith(ACTION_PREFIX)) ParseAction(message);
            else if (message.StartsWith(LIMIT_PRESSED_PREFIX)) ParseLimit(message, LimitType.Pressed);
            else if (message.StartsWith(LIMIT_RELEASED_PREFIX)) ParseLimit(message, LimitType.Released);
            else if (message.StartsWith(STEP_SIZE_PREFIX)) ParseStepSize(message);
            else if (message.StartsWith(MICROSTEPS_PREFIX)) ParseMicrosteps(message);
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
            var kv = ParseKV(msg, WELL_PREFIX);

            robotState.CurrentWell.Type = WellType.Standard;
            robotState.CurrentWell.Name = kv["Name"].ToUpper();
            robotState.CurrentWell.X = double.Parse(kv["X"], CultureInfo.InvariantCulture);
            robotState.CurrentWell.Y = double.Parse(kv["Y"], CultureInfo.InvariantCulture);
            robotState.CurrentWell.AngleL = kv["L"];
            robotState.CurrentWell.AngleR = kv["R"].Trim();

            robotState.Settings.State = MoveState.Idle;
        }

        private void ParsePosition(string msg)
        {
            var kv = ParseKV(msg, POS_PREFIX);

            robotState.Position.L = kv["L"];
            robotState.Position.R = kv["R"];
            robotState.Position.Z1 = kv["Z1"];
            robotState.Position.Z2 = kv["Z2"].Trim();

            if (robotState.Settings.State == MoveState.EmergencyStopped) return;

            // If we're getting position updates, we must be moving (unless we e-stopped)
            robotState.Settings.State = MoveState.Idle;
        }

        private void ParseCalPoint(string msg)
        {
            var kv = ParseKV(msg, CAL_PT_PREFIX);
            {
                string name = kv["Name"].ToUpper();
                CalibrationPoint existingPoint = robotState.Calibration.Points.FirstOrDefault(p => p.Name == name);
                if (existingPoint != null)
                {
                    existingPoint.X = (int)double.Parse(kv["X"], CultureInfo.InvariantCulture);
                    existingPoint.Y = (int)double.Parse(kv["Y"].Trim(), CultureInfo.InvariantCulture);
                    existingPoint.ErrorLeft = double.Parse(kv["ErrorLeft"], CultureInfo.InvariantCulture);
                    existingPoint.ErrorRight = double.Parse(kv["ErrorRight"].Trim(), CultureInfo.InvariantCulture);
                }
                else
                {
                    int x = (int)double.Parse(kv["X"], CultureInfo.InvariantCulture);
                    int y = (int)double.Parse(kv["Y"].Trim(), CultureInfo.InvariantCulture);
                    double? errorLeft = null;
                    double? errorRight = null;

                    if (kv.ContainsKey("ErrorLeft") && kv.ContainsKey("ErrorRight"))
                    {
                        errorLeft = double.Parse(kv["ErrorLeft"], CultureInfo.InvariantCulture);
                        errorRight = double.Parse(kv["ErrorRight"], CultureInfo.InvariantCulture);
                    }

                    CalibrationPoint point = new CalibrationPoint(name, x, y, errorLeft, errorRight);

                    robotState.Calibration.Points.Add(point);
                };
            }
        }

        private void ParseCalRecorded(string msg)
        {
            var kv = ParseKV(msg, CAL_REC_PREFIX);
            {

                string name = kv["Name"].ToUpper();
                int x = (int)double.Parse(kv["X"], CultureInfo.InvariantCulture);
                int y = (int)double.Parse(kv["Y"].Trim(), CultureInfo.InvariantCulture);

                CalibrationPoint existingPoint = robotState.Calibration.Points.FirstOrDefault(p => p.Name == name);
                if (existingPoint != null)
                {
                    existingPoint.X = (int)double.Parse(kv["X"], CultureInfo.InvariantCulture);
                    existingPoint.Y = (int)double.Parse(kv["Y"].Trim(), CultureInfo.InvariantCulture);
                }
                else
                {
                    CalibrationPoint point = new CalibrationPoint(name);

                    robotState.Calibration.Points.Add(point);
                }
            };
        }

        private void ParseRms(string msg)
        {
            var d = ParseKV(msg, RMS_PREFIX);
            robotState.Calibration.RmsL = double.Parse(d["L"], CultureInfo.InvariantCulture);
            robotState.Calibration.RmsR = double.Parse(d["R"], CultureInfo.InvariantCulture);
        }

        private void ParseAction(string msg)
        {
            var kv = ParseKV(msg, ACTION_PREFIX);
            int id = int.Parse(kv["Id"], CultureInfo.InvariantCulture);
            ActionType type = (ActionType)int.Parse(kv["Type"], CultureInfo.InvariantCulture);
            int pump = int.Parse(kv["Pump"], CultureInfo.InvariantCulture);
            int amount = int.Parse(kv["Amount"], CultureInfo.InvariantCulture);
            int frequency = int.Parse(kv["Frequency"], CultureInfo.InvariantCulture);
            TimeUnit unit = (TimeUnit)int.Parse(kv["Unit"], CultureInfo.InvariantCulture);
            long startTime = long.Parse(kv["StartTime"], CultureInfo.InvariantCulture);
            long endTime = long.Parse(kv["EndTime"], CultureInfo.InvariantCulture);

            ScheduledAction action = new ScheduledAction(id, type, pump, amount, frequency, unit, startTime, endTime);

            RobotState.ActionScheduler.Actions.Add(action);
        }

        private void ParseLimit(string msg, LimitType type)
        {
            string prefix = type == LimitType.Pressed ? LIMIT_PRESSED_PREFIX : LIMIT_RELEASED_PREFIX;
            bool state = type == LimitType.Pressed;

            var kv = ParseKV(msg, prefix);
            var axis = kv["AXIS"].Trim();

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

        private void ParseMicrosteps(string msg)
        {
            string d = msg.Substring(MICROSTEPS_PREFIX.Length).Trim();
            robotState.Settings.Microsteps = d;
        }
    }
}
