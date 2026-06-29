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
using System.Threading.Tasks;

namespace PHIL_GUI.Services
{
    /// <summary>
    /// Defines the limit switch states.
    /// </summary>
    enum LimitType
    {
        /// <summary>Limit switch is released (not pressed).</summary>
        Released,
        /// <summary>Limit switch is pressed (active).</summary>
        Pressed
    }
    /// <summary>
    /// Service for handling communication protocol with the PHIL robot hardware.
    /// Manages command sending, response parsing, state synchronization, and action execution.
    /// </summary>
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
        const string PRIME_CMD = "PRIME";
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
        const string PRINT_MAX_ACTIONS_CMD = "PRINT_MAX_ACTIONS";
        const string PRINT_MAX_ACTIONS_PER_WELL_CMD = "PRINT_MAX_ACTIONS_PER_WELL";
        const string PRINT_TIME_CMD = "PRINT_TIME";
        const string SET_TIME_CMD = "SET_TIME";
        const string SET_PLATE_TYPE_CMD = "SET_PLATE_TYPE";

        const string WELL_PREFIX = "WELL:";
        const string POS_PREFIX = "POS:";
        const string CAL_PT_PREFIX = "CAL_PT:";
        const string CAL_REC_PREFIX = "CAL_REC:";
        const string CAL_DEL_PREFIX = "CAL_DEL:";
        const string ACTION_PREFIX = "ACTION:";
        const string WELL_ACTION_PREFIX = "WELL_ACTION:";
        const string ACTION_CREATED_PREFIX = "ACTION_CREATED:";
        const string EXECUTING_ACTION_ID_PREFIX = "EXECUTING_ACTION_ID:";
        const string RMS_PREFIX = "RMS:";
        const string LIMIT_PRESSED_PREFIX = "LIMIT_PRESSED:";
        const string LIMIT_RELEASED_PREFIX = "LIMIT_RELEASED:";
        const string STEP_SIZE_PREFIX = "STEP_SIZE:";
        const string MICROSTEPS_PREFIX = "MICROSTEPS:";
        const string TIME_PREFIX = "TIME:";
        const string MAX_ACTIONS_TOTAL_PREIX = "MAX_ACTIONS_TOTAL:";
        const string MAX_ACTIONS_PER_WELL_PREIX = "MAX_ACTIONS_PER_WELL:";
        const string END_CALIBRATION = "END_CAL";
        const string END_ACTIONS = "END_ACTIONS";
        const string END_WELL_ACTIONS = "END_WELL_ACTIONS";


        /// <summary>
        /// Event raised when the application has completed initialization with the robot.
        /// </summary>
        public event Action OnAppInitialized;

        private TaskCompletionSource<bool>? waiter;
        private string? expectedCompletion;

        private bool ready;

        private readonly MediaService mediaService;
        /// <summary>
        /// Gets the media service for recording videos.
        /// </summary>
        public MediaService MediaService => mediaService;
        private readonly SerialPortService serialPortService = new SerialPortService();
        /// <summary>
        /// Gets the serial port service for hardware communication.
        /// </summary>
        public SerialPortService SerialPortService => serialPortService;
        private readonly RobotState robotState = new RobotState();
        /// <summary>
        /// Gets the current robot state including position, calibration, and actions.
        /// </summary>
        public RobotState RobotState => robotState;

        private readonly StringBuilder logBuffer = new();
        private readonly DispatcherTimer logTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };

        private string receivedData = "";
        /// <summary>
        /// Gets the log of received data from the robot for debugging purposes.
        /// </summary>
        public string ReceivedData
        {
            get => receivedData;
            private set => SetProperty(ref receivedData, value);
        }

        /// <summary>
        /// Initializes a new instance of the RobotProtocolService class.
        /// </summary>
        /// <param name="recordContext">Context for accessing recording settings.</param>
        public RobotProtocolService(IRecordContext recordContext)
        {
            mediaService = new MediaService(recordContext);

            logTimer.Tick += LogTimer_Tick;
            logTimer.Start();

            serialPortService.MessageReceived += OnMessageReceived;
            serialPortService.OnConnected += OnSerialPortConnected;
        }

        /// <summary>
        /// Handles the periodic timer tick event to flush the log buffer to ReceivedData.
        /// </summary>
        private void LogTimer_Tick(object? sender, EventArgs e)
        {
            if (logBuffer.Length <= 0) return;

            ReceivedData += logBuffer.ToString();
            logBuffer.Clear();
        }

        /// <summary>
        /// Sends a command to the robot via the serial port and logs it to ReceivedData.
        /// </summary>
        /// <param name="command">The command string to send.</param>
        private void SendCommand(string command)
        {
            ReceivedData += $"Sent To Robot: {command}\n";
            serialPortService.SendMessage(command);
        }

        /// <summary>
        /// Creates a new action on the robot and in the local action scheduler.
        /// </summary>
        /// <param name="action">The action to create.</param>
        public void CreateAction(ActionItem action)
        {
            SendCommand($"{CREATE_ACTION_CMD} {(int)action.TempId} {(int)action.Type} {(int)action.Pump1} {(int)action.Pump2} {action.Amount} {action.Frequency} {(int)action.TimeUnit} {action.StartEpoch} {action.EndEpoch}");
            RobotState.ActionScheduler.CreateAction(action);
        }

        /// <summary>
        /// Updates an existing action on the robot and in the local action scheduler.
        /// </summary>
        /// <param name="action">The action with updated values.</param>
        public void UpdateAction(ActionItem action)
        {
            SendCommand($"{UPDATE_ACTION_CMD} {action.Id} {(int)action.Type} {(int)action.Pump1} {(int)action.Pump2} {action.Amount} {action.Frequency} {(int)action.TimeUnit} {action.StartEpoch} {action.EndEpoch}");
            RobotState.ActionScheduler.UpdateAction(action);
        }

        /// <summary>
        /// Deletes an action from the robot and the local action scheduler.
        /// </summary>
        /// <param name="actionId">The ID of the action to delete.</param>
        public void DeleteAction(int actionId)
        {
            SendCommand($"{DEL_ACTION_CMD} {actionId}");
            RobotState.ActionScheduler.DeleteAction(actionId);
        }

        /// <summary>
        /// Attaches an action to multiple wells by their indices.
        /// </summary>
        /// <param name="action">The action to attach.</param>
        /// <param name="selectedWellIndices">The well indices to attach the action to.</param>
        public void AttachAction(ScheduleAction action, IEnumerable<int> selectedWellIndices)
        {
            byte[] bitmask = selectedWellIndices.WellIndicesToBitmask();
            string hex = BitConverter.ToString(bitmask).Replace("-", "");
            SendCommand($"{LINK_ACTION_WELL_CMD} {action.Id} {hex}");
            RobotState.ActionScheduler.AttachAction(action, selectedWellIndices);
        }
        /// <summary>
        /// Detaches an action from multiple wells by their indices.
        /// </summary>
        /// <param name="action">The action to detach.</param>
        /// <param name="selectedWellIndices">The well indices to detach the action from.</param>
        public void DetachAction(ScheduleAction action, IEnumerable<int> selectedWellIndices)
        {
            byte[] bitmask = selectedWellIndices.WellIndicesToBitmask();
            string hex = BitConverter.ToString(bitmask).Replace("-", "");
            SendCommand($"{UNLINK_ACTION_WELL_CMD} {action.Id} {hex}");
            RobotState.ActionScheduler.DetachAction(action, selectedWellIndices);
        }

        /// <summary>
        /// Clears the received data log.
        /// </summary>
        public void ClearReceivedData()
        {
            ReceivedData = "";
        }

        /// <summary>
        /// Sets the well plate type on the robot (OrganOnChip or Well96).
        /// </summary>
        /// <param name="plateType">The plate type to set.</param>
        public void SetWellPlateType(PlateType plateType)
        {
            SendCommand($"{SET_PLATE_TYPE_CMD} {plateType}");
        }

        /// <summary>
        /// Sends an emergency stop command to the robot.
        /// </summary>
        public void Stop()
        {
            SendCommand("s");
        }

        /// <summary>
        /// Moves the robot up (Z-axis positive direction).
        /// </summary>
        public void MoveUp()
        {
            SendCommand(MOVE_UP_CMD);
        }

        /// <summary>
        /// Moves the robot down (Z-axis negative direction).
        /// </summary>
        public void MoveDown()
        {
            SendCommand(MOVE_DOWN_CMD);
        }

        /// <summary>
        /// Moves the robot forward (Y-axis positive direction).
        /// </summary>
        public void MoveForward()
        {
            SendCommand(MOVE_FORWARD_CMD);
        }

        /// <summary>
        /// Moves the robot backward (Y-axis negative direction).
        /// </summary>
        public void MoveBackward()
        {
            SendCommand(MOVE_BACKWARD_CMD);
        }

        /// <summary>
        /// Moves the robot left (X-axis negative direction).
        /// </summary>
        public void MoveLeft()
        {
            SendCommand(MOVE_LEFT_CMD);
        }

        /// <summary>
        /// Moves the robot right (X-axis positive direction).
        /// </summary>
        public void MoveRight()
        {
            SendCommand(MOVE_RIGHT_CMD);
        }

        /// <summary>
        /// Decreases the movement step size.
        /// </summary>
        public void DecreaseStepSize()
        {
            SendCommand(DEC_STEP_CMD);
        }

        /// <summary>
        /// Increases the movement step size.
        /// </summary>
        public void IncreaseStepSize()
        {
            SendCommand(INC_STEP_CMD);
        }

        /// <summary>
        /// Records a calibration point at the current robot position for the specified well.
        /// </summary>
        /// <param name="wellName">The well name (e.g., "A1").</param>
        public void RecordCalibrationPoint(string wellName)
        {
            SendCommand($"{RECORD_POINT_CMD} {wellName}");
        }

        /// <summary>
        /// Solves the calibration map using the recorded calibration points.
        /// </summary>
        public void SolveMap()
        {
            SendCommand(SOLVE_MAP_CMD);
        }

        /// <summary>
        /// Deletes a specific calibration point.
        /// </summary>
        /// <param name="wellName">The well name of the calibration point to delete.</param>
        public void DeleteCalibrationPoint(string wellName)
        {
            SendCommand($"{DELETE_POINT_CMD} {wellName}");
        }

        /// <summary>
        /// Clears all calibration points from the robot.
        /// </summary>
        public void ClearCalibration()
        {
            SendCommand(CLEAR_CALIBRATION_CMD);
        }

        /// <summary>
        /// Aspirates (draws in) liquid using the specified pump.
        /// </summary>
        /// <param name="pumpNumber">The pump number (1-4).</param>
        /// <param name="volume">The volume in microliters to aspirate.</param>
        public void Aspirate(int pumpNumber, int volume)
        {
            SendCommand($"{ASPIRATE_CMD} {pumpNumber} {volume}");
        }

        /// <summary>
        /// Dispenses (expels) liquid using the specified pump.
        /// </summary>
        /// <param name="pumpNumber">The pump number (1-4).</param>
        /// <param name="volume">The volume in microliters to dispense.</param>
        public void Dispense(int pumpNumber, int volume)
        {
            SendCommand($"{DISPENSE_CMD} {pumpNumber} {volume}");
        }

        /// <summary>
        /// Primes the specified pump by dispensing the maximum volume.
        /// </summary>
        /// <param name="pumpNumber">The pump number (1-4) to prime.</param>
        public void Prime(int pumpNumber)
        {
            SendCommand($"{PRIME_CMD} {pumpNumber} {int.MaxValue}");
        }

        /// <summary>
        /// Moves the robot to a well using hardcoded coordinates.
        /// </summary>
        /// <param name="wellName">The well name (e.g., "A1").</param>
        public void MoveToHardcodedWell(string wellName)
        {
            SendCommand($"{MOVE_HARD_WELL_CMD} {wellName}");
        }

        /// <summary>
        /// Moves the robot to a well using calculated coordinates from calibration.
        /// </summary>
        /// <param name="wellName">The well name (e.g., "A1").</param>
        public void MoveToCalculatedWell(string wellName)
        {
            SendCommand($"{MOVE_CALC_WELL_CMD} {wellName}");
        }

        /// <summary>
        /// Moves the robot to the origin position.
        /// </summary>
        public void GoHome()
        {
            robotState.CurrentWell.Type = WellType.Home;
            robotState.Settings.State = MoveState.Moving;
            SendCommand(GO_HOME_CMD);
        }

        /// <summary>
        /// Calibrates the home position from the current robot location.
        /// </summary>
        public void CalibrateHome()
        {
            robotState.CurrentWell.Type = WellType.Home;
            robotState.Settings.State = MoveState.Moving;
            SendCommand(CALIBRATE_HOME_CMD);
        }

        /// <summary>
        /// Triggers an emergency stop, halting all robot movement immediately.
        /// </summary>
        public void EmergencyStop()
        {
            robotState.CurrentWell.Type = WellType.Unknown;
            robotState.Settings.State = MoveState.EmergencyStopped;
            SendCommand("s");
        }

        /// <summary>
        /// Handles the serial port connection event and initializes robot state by requesting all current data.
        /// </summary>
        private async void OnSerialPortConnected()
        {
            ready = true;

            await SendWithDelay(PRINT_WELL_CMD);
            await SendAndWait(PRINT_CALIBRATION_CMD, END_CALIBRATION);
            await SendWithDelay(PRINT_STEPS_CMD);
            await SendAndWait(PRINT_ACTIONS_CMD, END_ACTIONS);
            await SendAndWait(PRINT_WELL_ACTIONS_CMD, END_WELL_ACTIONS);
            await SendWithDelay(PRINT_TIME_CMD);
            await SendWithDelay(PRINT_MAX_ACTIONS_CMD);
            await SendWithDelay(PRINT_MAX_ACTIONS_PER_WELL_CMD);

            OnAppInitialized?.Invoke();
        }
        /// <summary>
        /// Sends a command with a delay to prevent overwhelming the serial communication.
        /// </summary>
        /// <param name="cmd">The command to send.</param>
        async Task SendWithDelay(string cmd)
        {
            SendCommand(cmd);
            await Task.Delay(50);
        }

        /// <summary>
        /// Sends a command and waits for a specific completion message before continuing.
        /// </summary>
        /// <param name="cmd">The command to send.</param>
        /// <param name="completion">The expected completion message.</param>
        async Task SendAndWait(string cmd, string completion)
        {
            expectedCompletion = completion;
            waiter = new TaskCompletionSource<bool>();

            SendCommand(cmd);

            await waiter.Task;
        }

        /// <summary>
        /// Handles received messages from the serial port, logging them and parsing their content.
        /// </summary>
        /// <param name="message">The received message.</param>
        private void OnMessageReceived(string message)
        {
            if (!ready) return;

            logBuffer.AppendLine($"{DateTime.Now:HH:mm:ss}: {message}");

            Dispatcher.UIThread.Post(() =>
            {
                ParseMessage(message);
            }, DispatcherPriority.Background);


            if (waiter == null || expectedCompletion == null) return;
            if (!message.Contains(expectedCompletion)) return;

            waiter.TrySetResult(true);
            waiter = null;
            expectedCompletion = null;
        }

        /// <summary>
        /// Parses a received message and routes it to the appropriate handler based on its prefix.
        /// </summary>
        /// <param name="message">The message to parse.</param>
        private void ParseMessage(string message)
        {
            if (message.StartsWith(WELL_PREFIX)) ParseWellArrival(message);
            else if (message.StartsWith(CAL_REC_PREFIX)) ParseCalRecorded(message);
            else if (message.StartsWith(CAL_DEL_PREFIX)) ParseCalDeleted(message);
            else if (message.StartsWith(CAL_PT_PREFIX)) ParseCalPoint(message);
            else if (message.StartsWith(POS_PREFIX)) ParsePosition(message);
            else if (message.StartsWith(RMS_PREFIX)) ParseRms(message);
            else if (message.StartsWith(ACTION_PREFIX)) ParseAction(message);
            else if (message.StartsWith(WELL_ACTION_PREFIX)) ParseWellAction(message);
            else if (message.StartsWith(ACTION_CREATED_PREFIX)) ParseActionCreated(message);
            else if (message.StartsWith(EXECUTING_ACTION_ID_PREFIX)) ParseActionExecution(message);
            else if (message.StartsWith(LIMIT_PRESSED_PREFIX)) ParseLimit(message, LimitType.Pressed);
            else if (message.StartsWith(LIMIT_RELEASED_PREFIX)) ParseLimit(message, LimitType.Released);
            else if (message.StartsWith(STEP_SIZE_PREFIX)) ParseStepSize(message);
            else if (message.StartsWith(MICROSTEPS_PREFIX)) ParseMicrosteps(message);
            else if (message.StartsWith(TIME_PREFIX)) ParseTime(message);
            else if (message.StartsWith(MAX_ACTIONS_TOTAL_PREIX)) ParseMaxActions(message);
            else if (message.StartsWith(MAX_ACTIONS_PER_WELL_PREIX)) ParseMaxWellActions(message);
        }

        /// <summary>
        /// Parses a key-value formatted message into a dictionary.
        /// </summary>
        /// <param name="msg">The message to parse.</param>
        /// <param name="prefix">The prefix to remove before parsing.</param>
        /// <returns>Dictionary of key-value pairs.</returns>
        private Dictionary<string, string> ParseKV(string msg, string prefix)
        {
            return msg.Substring(prefix.Length)
                      .Split(',')
                      .Select(p => p.Split('='))
                      .Where(p => p.Length == 2)
                                     .ToDictionary(p => p[0], p => p[1]);
                      }

        /// <summary>
        /// Parses a well arrival message and updates the robot's current well state.
        /// </summary>
        /// <param name="msg">The well arrival message.</param>
        private void ParseWellArrival(string msg)
        {
            var kv = ParseKV(msg, WELL_PREFIX);

            string name = kv["Name"].ToUpper();
            if (name == "HOME") robotState.CurrentWell.Type = WellType.Home;
            else if (name == "UNKNOWN") robotState.CurrentWell.Type = WellType.Unknown;
            else if (name == "CONTAINER") robotState.CurrentWell.Type = WellType.Container;
            else robotState.CurrentWell.Type = WellType.Standard;

            robotState.CurrentWell.Name = name;
            robotState.CurrentWell.X = double.Parse(kv["X"], CultureInfo.InvariantCulture);
            robotState.CurrentWell.Y = double.Parse(kv["Y"], CultureInfo.InvariantCulture);
            robotState.CurrentWell.AngleL = kv["L"];
            robotState.CurrentWell.AngleR = kv["R"].Trim();

            robotState.Settings.State = MoveState.Idle;
        }

        /// <summary>
        /// Parses a position update message and updates the robot's current position.
        /// </summary>
        /// <param name="msg">The position message.</param>
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

        /// <summary>
        /// Parses a calibration point message and updates or adds the calibration point in robot state.
        /// </summary>
        /// <param name="msg">The calibration point message.</param>
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

        /// <summary>
        /// Parses a calibration point recorded message and updates or adds the point to robot state.
        /// </summary>
        /// <param name="msg">The calibration recorded message.</param>
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

        /// <summary>
        /// Parses a calibration point deleted message and removes the point from robot state.
        /// </summary>
        /// <param name="msg">The calibration deleted message.</param>
        private void ParseCalDeleted(string msg)
        {
            var kv = ParseKV(msg, CAL_DEL_PREFIX);
            string name = kv["Name"].ToUpper();

            CalibrationPoint existingPoint = robotState.Calibration.Points.FirstOrDefault(p => p.Name == name);
            if (existingPoint == null) return;

            robotState.Calibration.Points.Remove(existingPoint);
        }

        /// <summary>
        /// Parses a root-mean-square (RMS) error message and updates calibration accuracy metrics.
        /// </summary>
        /// <param name="msg">The RMS message.</param>
        private void ParseRms(string msg)
        {
            var d = ParseKV(msg, RMS_PREFIX);
            robotState.Calibration.RmsL = double.Parse(d["L"], CultureInfo.InvariantCulture);
            robotState.Calibration.RmsR = double.Parse(d["R"], CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Parses a scheduled action message and adds the action to the action scheduler.
        /// </summary>
        /// <param name="msg">The action message.</param>
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
            long lastRunTime = long.Parse(kv["LastRun"], CultureInfo.InvariantCulture);

            ScheduleAction action = new ScheduleAction(id, type, pump1, pump2, amount, frequency, unit, startTime, endTime, lastRunTime);

            RobotState.ActionScheduler.Actions.Add(action);
        }

        /// <summary>
        /// Parses a well-action mapping message and associates actions with specific wells.
        /// </summary>
        /// <param name="msg">The well-action message.</param>
        private void ParseWellAction(string msg)
        {
            var kv = ParseKV(msg, WELL_ACTION_PREFIX);

            string actions = kv["Actions"];
            HashSet<int> actionIds = JsonSerializer.Deserialize<HashSet<int>>(actions);

            string well = kv["Well"];
            int wellIndex = well.ToIndex();

            RobotState.ActionScheduler.AddWellActions(actionIds, wellIndex);
        }

        /// <summary>
        /// Parses an action created confirmation message and updates the action ID mapping.
        /// </summary>
        /// <param name="msg">The action created message.</param>
        private void ParseActionCreated(string msg)
        {
            var kv = ParseKV(msg, ACTION_CREATED_PREFIX);
            int tempId = int.Parse(kv["TempId"], CultureInfo.InvariantCulture);
            int id = int.Parse(kv["Id"], CultureInfo.InvariantCulture);

            RobotState.ActionScheduler.UpdateAction(tempId, id);
        }

        /// <summary>
        /// Parses an action execution message, updates the last run time, and triggers video recording.
        /// </summary>
        /// <param name="msg">The action execution message.</param>
        private async void ParseActionExecution(string msg)
        {
            var kv = ParseKV(msg, EXECUTING_ACTION_ID_PREFIX);
            int id = int.Parse(kv["Id"], CultureInfo.InvariantCulture);
            long lastRunEpoch = long.Parse(kv["LastRun"], CultureInfo.InvariantCulture);
            RobotState.ActionScheduler.UpdateAction(id, lastRunEpoch);

            await MediaService.RecordVideo(id);
        }

        /// <summary>
        /// Parses a limit switch state message and updates the corresponding limit switch state.
        /// </summary>
        /// <param name="msg">The limit message.</param>
        /// <param name="type">The limit state type (pressed or released).</param>
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

        /// <summary>
        /// Parses a step size message and updates the robot settings.
        /// </summary>
        /// <param name="msg">The step size message.</param>
        private void ParseStepSize(string msg)
        {
            string stepSize = msg.Substring(STEP_SIZE_PREFIX.Length).Trim();
            robotState.Settings.StepSize = double.Parse(stepSize, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Parses a microsteps configuration message and updates the robot settings.
        /// </summary>
        /// <param name="msg">The microsteps message.</param>
        private void ParseMicrosteps(string msg)
        {
            string microSteps = msg.Substring(MICROSTEPS_PREFIX.Length).Trim();
            robotState.Settings.Microsteps = microSteps;
        }

        /// <summary>
        /// Parses a robot time message and synchronizes the robot clock if necessary.
        /// </summary>
        /// <param name="msg">The time message.</param>
        private void ParseTime(string msg)
        {
            string unixTimeStr = msg.Substring(TIME_PREFIX.Length).Trim();
            long unixTime = long.Parse(unixTimeStr, CultureInfo.InvariantCulture);

            bool isValid = robotState.ActionScheduler.IsRobotTimeValid(unixTime);

            if (isValid) return;

            SetTime(DateTimeOffset.Now.ToLocalTime().ToUnixTimeSeconds());
        }

        /// <summary>
        /// Parses the maximum total actions capacity message and updates the action scheduler limits.
        /// </summary>
        /// <param name="msg">The max actions message.</param>
        private void ParseMaxActions(string msg)
        {
            string maxActionsStr = msg.Substring(MAX_ACTIONS_TOTAL_PREIX.Length).Trim();
            int maxActions = int.Parse(maxActionsStr, CultureInfo.InvariantCulture);
            robotState.ActionScheduler.MaxTotalActions = maxActions;
        }

        /// <summary>
        /// Parses the maximum actions per well capacity message and updates the action scheduler limits.
        /// </summary>
        /// <param name="msg">The max well actions message.</param>
        private void ParseMaxWellActions(string msg)
        {
            string maxActionsStr = msg.Substring(MAX_ACTIONS_PER_WELL_PREIX.Length).Trim();
            int maxActions = int.Parse(maxActionsStr, CultureInfo.InvariantCulture);
            robotState.ActionScheduler.MaxActionsPerWell = maxActions;
        }

        /// <summary>
        /// Sends a command to set the robot's internal clock to the specified Unix timestamp.
        /// </summary>
        /// <param name="unixTime">The Unix timestamp in seconds.</param>
        private void SetTime(long unixTime)
        {
            SendCommand($"{SET_TIME_CMD} {unixTime}");
        }
    }
}
