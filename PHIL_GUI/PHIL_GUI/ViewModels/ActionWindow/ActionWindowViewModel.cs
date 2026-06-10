using Avalonia;
using CommunityToolkit.Mvvm.Input;
using PHIL_GUI.Models;
using PHIL_GUI.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace PHIL_GUI.ViewModels
{
    /// <summary>
    /// Defines the mode for the action window (creating a new action or updating an existing one).
    /// </summary>
    public enum ActionWindowMode
    {
        /// <summary>Creating a new action.</summary>
        Create,
        /// <summary>Updating an existing action.</summary>
        Update
    }

    /// <summary>
    /// ViewModel for the Action window, managing action creation and editing.
    /// Handles validation, date/time scheduling, and pump configuration.
    /// </summary>
    public class ActionWindowViewModel : ViewModelBase
    {
        private const int WINDOW_HEIGHT_NORMAL = 510;
        private const int ERROR_HEIGHT = 60;
        private const int REPEAT_HEIGHT = 130;

        /// <summary>
        /// Gets the command to save the action (create or update).
        /// </summary>
        public ICommand SaveCommand { get; }

        /// <summary>
        /// Gets the command to clear the start date and time.
        /// </summary>
        public ICommand ClearStartCommand { get; }

        /// <summary>
        /// Gets the command to clear the end date and time.
        /// </summary>
        public ICommand ClearEndCommand { get; }

        /// <summary>
        /// Gets the action item being created or edited.
        /// </summary>
        public ActionItem ActionItem { get; }

        /// <summary>
        /// Gets the mode of the window (Create or Update).
        /// </summary>
        public ActionWindowMode Mode { get; }

        /// <summary>
        /// Gets the available action types for selection.
        /// </summary>
        public IEnumerable<ActionType> ActionTypes { get; }

        /// <summary>
        /// Gets or sets the currently selected action type.
        /// </summary>
        public ActionType SelectedActionType { get; set; }

        /// <summary>
        /// Gets the available pumps for selection.
        /// </summary>
        public IEnumerable<Pump> Pumps { get; }

        /// <summary>
        /// Gets the available time units for scheduling frequency.
        /// </summary>
        public IEnumerable<TimeUnit> TimeUnits { get; }

        /// <summary>
        /// Gets the text for the save button based on the window mode.
        /// </summary>
        public string SaveButtonText => Mode == ActionWindowMode.Create ? "Create Action" : "Save Changes";

        /// <summary>
        /// Gets the label text for Pump 1 based on the plate type.
        /// </summary>
        public string Pump1Text => AppSettings.Is96Well ? $"PUMP" : $"PUMP IN";

        /// <summary>
        /// Gets the label text for Pump 2 based on the plate type.
        /// </summary>
        public string Pump2Text => AppSettings.Is96Well ? $"" : $"PUMP OUT";

        /// <summary>
        /// Gets the section label for date/time based on whether the action is repeating.
        /// </summary>
        public string DateTimeSectionLabel => IsRepeating ? "Schedule" : "When";

        /// <summary>
        /// Gets the label for the start date/time field based on whether the action is repeating.
        /// </summary>
        public string StartLabel => IsRepeating ? "START DATE & TIME" : "EXECUTE AT DATE & TIME";

        /// <summary>
        /// Gets the margin for Pump 1 control based on the plate type.
        /// </summary>
        public Thickness Pump1Margin => AppSettings.Is96Well ? new Thickness(6, 0, 0, 0) : new Thickness(0, 0, 6, 0);

        /// <summary>
        /// Gets the grid column for Pump 1 control based on the plate type.
        /// </summary>
        public int Pump1Column => AppSettings.Is96Well ? 1 : 0;

        private bool isRepeating = true;
        /// <summary>
        /// Gets or sets a value indicating whether the action is repeating.
        /// When changed, adjusts the window height and action frequency.
        /// </summary>
        public bool IsRepeating
        {
            get => isRepeating;
            set
            {
                if (isRepeating == value) return;

                if (value)
                {
                    ActionItem.Frequency = 1;
                    WindowHeight += REPEAT_HEIGHT;
                }
                else
                {
                    ActionItem.Frequency = -1;
                    WindowHeight -= REPEAT_HEIGHT;
                }


                SetProperty(ref isRepeating, value);

                OnPropertyChanged(nameof(StartLabel));
                OnPropertyChanged(nameof(DateTimeSectionLabel));
            }
        }

        private bool displayError;
        /// <summary>
        /// Gets or sets a value indicating whether an error message should be displayed.
        /// When changed, adjusts the window height to accommodate the error message.
        /// </summary>
        public bool DisplayError
        {
            get => displayError;
            set
            {
                if (value == displayError) return;

                if (value) WindowHeight += ERROR_HEIGHT;
                else WindowHeight -= ERROR_HEIGHT;

                SetProperty(ref displayError, value);
            }
        }

        private string errorMessage = string.Empty;
        /// <summary>
        /// Gets or sets the error message text to display to the user.
        /// </summary>
        public string ErrorMessage
        {
            get => errorMessage;
            set
            {
                if (value == errorMessage) return;

                SetProperty(ref errorMessage, value);
            }
        }

        private int windowHeight = WINDOW_HEIGHT_NORMAL;
        /// <summary>
        /// Gets or sets the height of the action window.
        /// Adjusts dynamically based on whether error messages or repeat options are shown.
        /// </summary>
        public int WindowHeight
        {
            get => windowHeight;
            set
            {
                if (value ==  windowHeight) return;

                SetProperty(ref windowHeight, value);
            }
        }

        /// <summary>
        /// Gets the application settings including plate type selection.
        /// </summary>
        public AppSettings AppSettings => AppSettingsService.AppSettings;

        /// <summary>
        /// Initializes a new instance of the ActionWindowViewModel class for creating or updating an action.
        /// </summary>
        /// <param name="mode">The mode (Create or Update).</param>
        /// <param name="action">The action item to edit (for Update mode) or null (for Create mode).</param>
        public ActionWindowViewModel(ActionWindowMode mode, ActionItem action)
        {
            Mode = mode;

            SaveCommand = new RelayCommand(Save);
            ClearStartCommand = new RelayCommand(ClearStart);
            ClearEndCommand = new RelayCommand(ClearEnd);

            ActionTypes = Enum.GetValues<ActionType>().Cast<ActionType>();
            if (AppSettings.Is96Well) ActionTypes = ActionTypes.SkipLast(1);
            Pumps = Enum.GetValues<Pump>().Cast<Pump>();
            TimeUnits = Enum.GetValues<TimeUnit>().Cast<TimeUnit>();

            if (mode == ActionWindowMode.Create)
            {
                int tempId = RobotProtocolService.RobotState.ActionScheduler.GetNextTempId();
                ActionType selectedActionType = ActionTypes.First();
                Pump selectedPump1 = Pump.P1;
                Pump selectedPump2 = Pump.P2;
                int amount = 50;
                TimeUnit selectedTimeUnit = TimeUnits.First();
                int frequency = 1;

                if (!AppSettings.Is96Well) selectedActionType = ActionType.Exchange;

                ActionItem = new ActionItem(tempId, selectedActionType, selectedPump1, selectedPump2, amount, frequency, selectedTimeUnit);
            }

            if (mode == ActionWindowMode.Update)
            {
                ActionItem = new ActionItem(action);
                IsRepeating = ActionItem.Frequency != -1;
            }
        }

        /// <summary>
        /// Initializes a new instance of the ActionWindowViewModel class for design-time preview.
        /// Creates dummy data for XAML designer.
        /// </summary>
        public ActionWindowViewModel()
        {
            // Dummy data for preview
            ActionTypes = Enum.GetValues<ActionType>();
            Pumps = Enum.GetValues<Pump>();
            TimeUnits = Enum.GetValues<TimeUnit>();

            ActionType selectedActionType = ActionTypes.First();
            Pump selectedPump1 = Pumps.First();
            Pump selectedPump2 = Pumps.Skip(1).First();
            int amount = 50;
            int frequency = 1;
            TimeUnit selectedTimeUnit = TimeUnits.First();

            ActionItem = new ActionItem(-1, selectedActionType, selectedPump1, selectedPump2, amount, frequency, selectedTimeUnit);
        }

        /// <summary>
        /// Clears the start date and time for the action.
        /// </summary>
        public void ClearStart()
        {
            ActionItem.StartEpoch = 0;
            ActionItem.StartTime = null;
            ActionItem.StartDate = null;
        }

        /// <summary>
        /// Clears the end date and time for the action.
        /// </summary>
        public void ClearEnd()
        {
            ActionItem.EndEpoch = 0;
            ActionItem.EndTime = null;
            ActionItem.EndDate = null;
        }

        /// <summary>
        /// Validates and saves the action, creating a new action or updating an existing one.
        /// </summary>
        public void Save()
        {
            if (!IsFormValid()) return;

            ActionItem.ConvertToEpoch();

            if (Mode == ActionWindowMode.Create)
            {
                CreateAction(ActionItem);
            }
            
            if (Mode == ActionWindowMode.Update)
            {
                UpdateAction(ActionItem);
            }

            if (DisplayError) DisplayError = false;

        }

        /// <summary>
        /// Validates the action form data.
        /// Checks that pumps are different (for organ-on-chip) and that start/end times are valid.
        /// </summary>
        /// <returns>True if the form is valid, otherwise false.</returns>
        private bool IsFormValid()
        {
            if (!AppSettings.Is96Well && (ActionItem.Pump1 == ActionItem.Pump2) && (ActionItem.Pump1 != Pump.None))
            {
                SetError("Pump 1 and Pump 2 cannot be the same.");
                return false;
            }

            if (ActionItem.StartDate.HasValue && ActionItem.EndDate.HasValue)
            {
                var startDate = ActionItem.StartDate.Value;
                var startTime = ActionItem.StartTime.Value;

                var endDate = ActionItem.EndDate.Value;
                var endTime = ActionItem.EndTime.Value;

                DateTimeOffset start = new DateTimeOffset(
                    startDate.Year,
                    startDate.Month,
                    startDate.Day,
                    startTime.Hours,
                    startTime.Minutes,
                    startTime.Seconds,
                    startDate.Offset);

                DateTimeOffset end = new DateTimeOffset(
                    endDate.Year,
                    endDate.Month,
                    endDate.Day,
                    endTime.Hours,
                    endTime.Minutes,
                    endTime.Seconds,
                    endDate.Offset);


                if ((end - start).TotalMinutes < 1)
                {
                    SetError("Start and end must be at least 1 minute apart.");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Sets an error message and displays it to the user.
        /// </summary>
        /// <param name="message">The error message to display.</param>
        private void SetError(string message)
        {
            DisplayError = true;
            ErrorMessage = message;
        }

        /// <summary>
        /// Creates a new action on the robot.
        /// </summary>
        /// <param name="actionItem">The action item to create.</param>
        private void CreateAction(ActionItem actionItem)
        {
            RobotProtocolService.CreateAction(actionItem);
        }

        /// <summary>
        /// Updates an existing action on the robot.
        /// </summary>
        /// <param name="actionItem">The action item with updated values.</param>
        private void UpdateAction(ActionItem actionItem)
        {
            if (ActionItem == null) return;

            RobotProtocolService.UpdateAction(ActionItem);
        }
    }
}
