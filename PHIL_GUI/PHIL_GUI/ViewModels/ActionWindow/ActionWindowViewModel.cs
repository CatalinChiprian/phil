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
    public enum ActionWindowMode
    {
        Create,
        Update
    }
    public class ActionWindowViewModel : ViewModelBase
    {
        private const int WINDOW_HEIGHT_NORMAL = 510;
        private const int ERROR_HEIGHT = 60;
        private const int REPEAT_HEIGHT = 130;

        public ICommand SaveCommand { get; }
        public ICommand ClearStartCommand { get; }
        public ICommand ClearEndCommand { get; }

        public ActionItem ActionItem { get; }
        public ActionWindowMode Mode { get; }
        public IEnumerable<ActionType> ActionTypes { get; }
        public ActionType SelectedActionType { get; set; }
        public IEnumerable<Pump> Pumps { get; }
        public IEnumerable<TimeUnit> TimeUnits { get; }

        public string SaveButtonText => Mode == ActionWindowMode.Create ? "Create Action" : "Save Changes";
        public string Pump1Text => AppSettings.Is96Well ? $"PUMP" : $"PUMP IN";
        public string Pump2Text => AppSettings.Is96Well ? $"" : $"PUMP OUT";
        public string DateTimeSectionLabel => IsRepeating ? "Schedule" : "When";
        public string StartLabel => IsRepeating ? "START DATE & TIME" : "EXECUTE AT DATE & TIME";
        public Thickness Pump1Margin => AppSettings.Is96Well ? new Thickness(6, 0, 0, 0) : new Thickness(0, 0, 6, 0);
        public int Pump1Column => AppSettings.Is96Well ? 1 : 0;

        private bool isRepeating = true;
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
        public int WindowHeight
        {
            get => windowHeight;
            set
            {
                if (value ==  windowHeight) return;

                SetProperty(ref windowHeight, value);
            }
        }

        public AppSettings AppSettings => AppSettingsService.AppSettings;

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

        public void ClearStart()
        {
            ActionItem.StartEpoch = 0;
            ActionItem.StartTime = null;
            ActionItem.StartDate = null;
        }
        public void ClearEnd()
        {
            ActionItem.EndEpoch = 0;
            ActionItem.EndTime = null;
            ActionItem.EndDate = null;
        }
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

        private void SetError(string message)
        {
            DisplayError = true;
            ErrorMessage = message;
        }

        private void CreateAction(ActionItem actionItem)
        {
            RobotProtocolService.CreateAction(actionItem);
        }

        private void UpdateAction(ActionItem actionItem)
        {
            if (ActionItem == null) return;

            RobotProtocolService.UpdateAction(ActionItem);
        }
    }
}
