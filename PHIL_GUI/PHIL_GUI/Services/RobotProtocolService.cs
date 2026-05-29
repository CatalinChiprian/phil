using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using PHIL_GUI.Helpers;
using PHIL_GUI.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;

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
        const string CLEAR_ACTIONS_CMD = "CLEAR_ACTIONS";
        const string PRINT_ACTIONS_CMD = "PRINT_ACTIONS";
        const string PRINT_WELL_ACTIONS_CMD = "PRINT_WELL_ACTIONS";
        const string PRINT_TIME_CMD = "PRINT_TIME";
        const string SET_TIME_CMD = "SET_TIME";
        const string SET_PLATE_TYPE_CMD = "SET_PLATE_TYPE";

        const string WELL_PREFIX = "WELL:";
        const string POS_PREFIX = "POS:";
        const string CAL_PT_PREFIX = "CAL_PT:";
        const string CAL_REC_PREFIX = "CAL_REC:";
        const string ACTION_PREFIX = "ACTION:";
        const string WELL_ACTION_PREFIX = "WELL_ACTION:";
        const string ACTION_CREATED_PREFIX = "ACTION_CREATED:";
        const string RMS_PREFIX = "RMS:";
        const string LIMIT_PRESSED_PREFIX = "LIMIT_PRESSED:";
        const string LIMIT_RELEASED_PREFIX = "LIMIT_RELEASED:";
        const string STEP_SIZE_PREFIX = "STEP_SIZE:";
        const string MICROSTEPS_PREFIX = "MICROSTEPS:";
        const string TIME_PREFIX = "TIME:";

        private bool ready;

        private readonly SerialPortService serialPortService = new SerialPortService();
        public SerialPortService SerialPort => serialPortService;
        private readonly RobotState robotState = new RobotState();
        public RobotState RobotState => robotState;

        private readonly StringBuilder logBuffer = new();
        private readonly DispatcherTimer logTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };

        private string receivedData = "";
        public string ReceivedData
        {
            get => receivedData;
            private set => SetProperty(ref receivedData, value);
        }

        public RobotProtocolService()
        {
            logTimer.Tick += LogTimer_Tick;
            logTimer.Start();

            serialPortService.MessageReceived += OnMessageReceived;
            serialPortService.OnConnected += OnSerialPortConnected;
        }

        private void LogTimer_Tick(object? sender, EventArgs e)
        {
            if (logBuffer.Length <= 0) return;

            ReceivedData += logBuffer.ToString();
            logBuffer.Clear();
        }

        private void Send(string command)
        {
            serialPortService.SendMessage(command);
        }

        public void CreateAction(ActionItem action)
        {
            Send($"{CREATE_ACTION_CMD} {(int)action.TempId} {(int)action.Type} {(int)action.Pump1} {(int)action.Pump2} {action.Amount} {action.Frequency} {(int)action.TimeUnit} {action.StartEpoch} {action.EndEpoch}");
            RobotState.ActionScheduler.CreateAction(action);
        }

        public void UpdateAction(ActionItem action)
        {
            Send($"{UPDATE_ACTION_CMD} {action.Id} {(int)action.Type} {(int)action.Pump1} {(int)action.Pump2} {action.Amount} {action.Frequency} {(int)action.TimeUnit} {action.StartEpoch} {action.EndEpoch}");
            RobotState.ActionScheduler.UpdateAction(action);
        }

        public void DeleteAction(int actionId)
        {
            Send($"{DEL_ACTION_CMD} {actionId}");
            RobotState.ActionScheduler.DeleteAction(actionId);
        }

        public void AttachAction(ScheduleAction action, IEnumerable<int> selectedWellIndices)
        {
            byte[] bitmask = selectedWellIndices.WellIndicesToBitmask();
            string hex = BitConverter.ToString(bitmask).Replace("-", "");
            Send($"{LINK_ACTION_WELL_CMD} {action.Id} {hex}");
            RobotState.ActionScheduler.AttachAction(action, selectedWellIndices);
        }
        public void DetachAction(ScheduleAction action, IEnumerable<int> selectedWellIndices)
        {
            byte[] bitmask = selectedWellIndices.WellIndicesToBitmask();
            string hex = BitConverter.ToString(bitmask).Replace("-", "");
            Send($"{UNLINK_ACTION_WELL_CMD} {action.Id} {hex}");
            RobotState.ActionScheduler.DetachAction(action, selectedWellIndices);
        }

        public void ClearReceivedData()
        {
            ReceivedData = "";
        }

        public void SetWellPlateType(PlateType plateType)
        {
            Send($"{SET_PLATE_TYPE_CMD} {plateType}");
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

        private void OnSerialPortConnected()
        {
            ready = true;

            Send(PRINT_WELL_CMD);
            Send(PRINT_CALIBRATION_CMD);
            Send(PRINT_STEPS_CMD);
            Send(PRINT_ACTIONS_CMD);
            Send(PRINT_WELL_ACTIONS_CMD);
            Send(PRINT_TIME_CMD);
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
            else if (message.StartsWith(WELL_ACTION_PREFIX)) ParseWellAction(message);
            else if (message.StartsWith(ACTION_CREATED_PREFIX)) ParseActionCreated(message);
            else if (message.StartsWith(LIMIT_PRESSED_PREFIX)) ParseLimit(message, LimitType.Pressed);
            else if (message.StartsWith(LIMIT_RELEASED_PREFIX)) ParseLimit(message, LimitType.Released);
            else if (message.StartsWith(STEP_SIZE_PREFIX)) ParseStepSize(message);
            else if (message.StartsWith(MICROSTEPS_PREFIX)) ParseMicrosteps(message);
            else if (message.StartsWith(TIME_PREFIX)) ParseTime(message);
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
            ActionType type = (ActionType)int.Parse(kv["ActionType"], CultureInfo.InvariantCulture);
            Pump pump1 = (Pump)int.Parse(kv["Pump1"], CultureInfo.InvariantCulture);
            Pump pump2 = (Pump)int.Parse(kv["Pump2"], CultureInfo.InvariantCulture);
            int amount = int.Parse(kv["Amount"], CultureInfo.InvariantCulture);
            int frequency = int.Parse(kv["Frequency"], CultureInfo.InvariantCulture);
            TimeUnit unit = (TimeUnit)int.Parse(kv["Unit"], CultureInfo.InvariantCulture);
            long startTime = long.Parse(kv["Start"], CultureInfo.InvariantCulture);
            long endTime = long.Parse(kv["End"], CultureInfo.InvariantCulture);

            ScheduleAction action = new ScheduleAction(id, type, pump1, pump2, amount, frequency, unit, startTime, endTime);

            RobotState.ActionScheduler.Actions.Add(action);
        }

        private void ParseWellAction(string msg)
        {
            var kv = ParseKV(msg, WELL_ACTION_PREFIX);

            string actions = kv["Actions"];
            HashSet<int> actionIds = JsonSerializer.Deserialize<HashSet<int>>(actions);

            string well = kv["Well"];
            int wellIndex = well.ToIndex();

            RobotState.ActionScheduler.AddWellActions(actionIds, wellIndex);
        }

        private void ParseActionCreated(string msg)
        {
            var kv = ParseKV(msg, ACTION_CREATED_PREFIX);
            int tempId = int.Parse(kv["TempId"], CultureInfo.InvariantCulture);
            int id = int.Parse(kv["Id"], CultureInfo.InvariantCulture);

            RobotState.ActionScheduler.UpdateAction(tempId, id);
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
            string stepSize = msg.Substring(STEP_SIZE_PREFIX.Length).Trim();
            robotState.Settings.StepSize = double.Parse(stepSize, CultureInfo.InvariantCulture);
        }

        private void ParseMicrosteps(string msg)
        {
            string microSteps = msg.Substring(MICROSTEPS_PREFIX.Length).Trim();
            robotState.Settings.Microsteps = microSteps;
        }

        private void ParseTime(string msg)
        {
            string unixTimeStr = msg.Substring(TIME_PREFIX.Length).Trim();
            long unixTime = long.Parse(unixTimeStr, CultureInfo.InvariantCulture);

            bool isValid = robotState.ActionScheduler.IsRobotTimeValid(unixTime);

            if (isValid) return;

            SetTime(DateTimeOffset.Now.ToLocalTime().ToUnixTimeSeconds());
        }

        private void SetTime(long unixTime)
        {
            Send($"{SET_TIME_CMD} {unixTime}");
        }
    }
}
