using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;

namespace PHIL_GUI.Models
{
    /// <summary>
    /// Represents a scheduled action item for robotic liquid handling operations.
    /// Supports aspirate, dispense, and exchange operations with configurable scheduling and UI binding.
    /// </summary>
    public class ActionItem : ObservableObject, IAction
    {
        /// <summary>
        /// Gets or sets the underlying schedule action model.
        /// </summary>
        public ScheduleAction Model { get; set; }
        private int id;
        /// <summary>
        /// Gets or sets the unique identifier for this action.
        /// </summary>
        public int Id
        {
            get => id;
            set
            {
                if (value == id) return;
                SetProperty(ref id, value);
            }
        }
        /// <summary>
        /// Gets or sets the temporary identifier used before the action is persisted.
        /// </summary>
        public int TempId { get; set; }

        private ActionType type;
        /// <summary>
        /// Gets or sets the type of action (Aspirate, Dispense, or Exchange).
        /// Changes trigger updates to related UI properties.
        /// </summary>
        public ActionType Type
        {
            get => type;
            set
            {
                if (value == type) return;
                SetProperty(ref type, value);
                OnPropertyChanged(nameof(ActionTypeLabel));
                OnPropertyChanged(nameof(Pump2Label));
                OnPropertyChanged(nameof(ActionTypeBackgroundColor));
                OnPropertyChanged(nameof(ActionTypeBorderColor));
                OnPropertyChanged(nameof(ActionTypeTextColor));
                OnPropertyChanged(nameof(Summary));
            }
        }
        /// <summary>
        /// Gets a short label for the action type (ASP, DISP, or EXCH).
        /// </summary>
        public string ActionTypeLabel
        {
            get
            {
                return Type switch
                {
                    ActionType.Aspirate => "ASP",
                    ActionType.Dispense => "DISP",
                    ActionType.Exchange => "EXCH",
                    _ => ""
                };
            }
        }
        private string actionTypeLabelBackgroundColor
        {
            get
            {
                return Type switch
                {
                    ActionType.Aspirate => "Accent-Dim",
                    ActionType.Dispense => "Info-Dim",
                    ActionType.Exchange => "Exchange-Dim",
                    _ => ""
                };
            }
        }
        /// <summary>
        /// Gets the background color brush for the action type label based on the action type.
        /// </summary>
        public IBrush ActionTypeBackgroundColor => Application.Current.Resources[actionTypeLabelBackgroundColor] as IBrush;

        private string actionTypeLabelBorderColor
        {
            get
            {
                return Type switch
                {
                    ActionType.Aspirate => "Accent-Mid",
                    ActionType.Dispense => "Info-Mid",
                    ActionType.Exchange => "Exchange-Mid",
                    _ => ""
                };
            }
        }
        /// <summary>
        /// Gets the border color brush for the action type label based on the action type.
        /// </summary>
        public IBrush ActionTypeBorderColor => Application.Current.Resources[actionTypeLabelBorderColor] as IBrush;

        private string actionTypeTextColor {
            get
            {
                return Type switch
                {
                    ActionType.Aspirate => "Accent",
                    ActionType.Dispense => "Info",
                    ActionType.Exchange => "Exchange",
                    _ => ""
                };
            }
        }
        /// <summary>
        /// Gets the text color brush for the action type label based on the action type.
        /// </summary>
        public IBrush ActionTypeTextColor => Application.Current.Resources[actionTypeTextColor] as IBrush;

        private Pump pump1;
        /// <summary>
        /// Gets or sets the primary pump for the action.
        /// For Exchange actions, this is the source pump.
        /// </summary>
        public Pump Pump1
        {
            get => pump1;
            set
            {
                if (value == pump1) return;
                SetProperty(ref pump1, value);
                OnPropertyChanged(nameof(Summary));
            }
        }
        /// <summary>
        /// Gets the display label for Pump1.
        /// </summary>
        public string Pump1Label => GetPumpName(pump1);

        private Pump pump2;
        /// <summary>
        /// Gets or sets the secondary pump for Exchange actions.
        /// This is the destination pump for Exchange operations.
        /// </summary>
        public Pump Pump2
        {
            get => pump2;
            set
            {
                if (value == pump2) return;
                SetProperty(ref pump2, value);
                OnPropertyChanged(nameof(Summary));
            }
        }
        /// <summary>
        /// Gets the display label for Pump2. Only shown for Exchange actions.
        /// </summary>
        public string Pump2Label => Type == ActionType.Exchange ? GetPumpName(pump2) : "";
        private string GetPumpName(Pump pump) => pump == Pump.None ? "" : $"{pump.ToString()}";

        private int amount;
        /// <summary>
        /// Gets or sets the liquid volume amount in microliters (µL).
        /// </summary>
        public int Amount
        {
            get => amount;
            set
            {
                if (value == amount) return;
                SetProperty(ref amount, value);
                OnPropertyChanged(nameof(Summary));
            }
        }

        private int frequency;
        /// <summary>
        /// Gets or sets how often the action repeats. Set to -1 for one-time actions.
        /// </summary>
        public int Frequency
        {
            get => frequency;
            set
            {
                if (value == frequency) return;
                SetProperty(ref frequency, value);
                OnPropertyChanged(nameof(FrequencyLabel));
            }
        }

        /// <summary>
        /// Gets the display label for the frequency (e.g., "Every 5 Minutes").
        /// </summary>
        public string FrequencyLabel => Frequency != -1 ? $"Every {Frequency} {GetTimeUnitLabel()}" : "";

        private TimeUnit timeUnit;
        /// <summary>
        /// Gets or sets the time unit for the frequency (Minute, Hour, or Day).
        /// </summary>
        public TimeUnit TimeUnit
        {
            get => timeUnit;
            set
            {
                if (value == timeUnit) return;
                SetProperty(ref timeUnit, value);
                OnPropertyChanged(nameof(FrequencyLabel));
            }
        }
        private string GetTimeUnitLabel() =>
            (TimeUnit switch
            {
                TimeUnit.Minute => "Minute",
                TimeUnit.Hour => "Hour",
                TimeUnit.Day => "Day",
                _ => ""
            }) + (Frequency == 1 ? "" : "s");

        private long startEpoch;
        /// <summary>
        /// Gets or sets the start time as Unix epoch seconds.
        /// </summary>
        public long StartEpoch
        {
            get => startEpoch;
            set
            {
                if (value == startEpoch) return;
                SetProperty(ref startEpoch, value);
            }
        }

        private long endEpoch;
        /// <summary>
        /// Gets or sets the end time as Unix epoch seconds.
        /// </summary>
        public long EndEpoch
        {
            get => endEpoch;
            set
            {
                if (value == endEpoch) return;
                SetProperty(ref endEpoch, value);
            }
        }

        private long lastRunEpoch;
        /// <summary>
        /// Gets or sets the last execution time as Unix epoch seconds.
        /// </summary>
        public long LastRunEpoch
        {
            get => lastRunEpoch;
            set
            {
                if (value == lastRunEpoch) return;
                SetProperty(ref lastRunEpoch, value);
            }
        }

        private DateTimeOffset? startDate;
        /// <summary>
        /// Gets or sets the start date for the action.
        /// Includes validation to prevent dates in the past and auto-adjusts related properties.
        /// </summary>
        public DateTimeOffset? StartDate
        {
            get => startDate;
            set
            {
                if (value == startDate) return;

                if (!suppressValidation && value < Today.Date) value = Today.Date;
                if (startDate == null && startTime == null) SetProperty(ref startTime, Today.TimeOfDay, nameof(StartTime));

                // Since DatetimePicker has LocalValue priority the UI might still receive the incorrect value, so we need to reset it to null first to ensure the correct value is displayed
                if (Equals(startDate, value)) SetProperty(ref startDate, null);

                SetProperty(ref startDate, value);

                if (EndDate.HasValue && value > EndDate.Value.Date) EndDate = value;

                OnPropertyChanged(nameof(PeriodLabel));
            }
        }

        private TimeSpan? startTime;
        /// <summary>
        /// Gets or sets the start time of day for the action.
        /// Includes validation to prevent times in the past for today's date.
        /// </summary>
        public TimeSpan? StartTime
        {
            get => startTime;
            set
            {
                if (value == startTime) return;

                if (!suppressValidation && (StartDate?.Date == Today.Date && value < Today.TimeOfDay)) value = Today.TimeOfDay;

                // Since DatetimePicker has LocalValue priority the UI might still receive the incorrect value, so we need to reset it to null first to ensure the correct value is displayed
                if (Equals(startTime, value)) SetProperty(ref startTime, null);

                SetProperty(ref startTime, value);

                if (startDate == null) StartDate = Today.Date;

                if (EndTime.HasValue && value > EndTime.Value) EndTime = value;

                OnPropertyChanged(nameof(PeriodLabel));
            }
        }

        private DateTimeOffset? endDate;
        /// <summary>
        /// Gets or sets the end date for the action.
        /// Includes validation to ensure it's not before the start date.
        /// </summary>
        public DateTimeOffset? EndDate
        {
            get => endDate;
            set
            {
                if (value == endDate) return;

                if (!suppressValidation && (value < StartDate?.Date || value < Today.Date)) value = StartDate?.Date ?? Today.Date;
                if (endDate == null && endTime == null) SetProperty(ref endTime, Today.TimeOfDay, nameof(EndTime));

                // Since DatetimePicker has LocalValue priority the UI might still receive the incorrect value, so we need to reset it to null first to ensure the correct value is displayed
                if (Equals(endDate, value)) SetProperty(ref endDate, null);

                SetProperty(ref endDate, value);
                OnPropertyChanged(nameof(PeriodLabel));
            }
        }

        private TimeSpan? endTime;
        /// <summary>
        /// Gets or sets the end time of day for the action.
        /// Includes validation to ensure it's not before the start time on the same date.
        /// </summary>
        public TimeSpan? EndTime
        {
            get => endTime;
            set
            {
                if (value == endTime) return;

                if (StartDate.HasValue && EndDate?.Date >= StartDate?.Date && value < StartTime) value = StartTime;
                if (!suppressValidation && value < Today.TimeOfDay) value = Today.TimeOfDay;

                // Since DatetimePicker has LocalValue priority the UI might still receive the incorrect value, so we need to reset it to null first to ensure the correct value is displayed
                if (Equals(endTime, value)) SetProperty(ref endTime, null);

                SetProperty(ref endTime, value);

                if (endDate == null) EndDate = Today;

                OnPropertyChanged(nameof(PeriodLabel));
            }
        }

        private TimeSpan timeUntilNextRun;
        /// <summary>
        /// Gets or sets the time remaining until the next execution of this action.
        /// </summary>
        public TimeSpan TimeUntilNextRun
        {
            get => timeUntilNextRun;
            set => SetProperty(ref timeUntilNextRun, value);
        }

        /// <summary>
        /// Gets the current local date/time.
        /// </summary>
        public DateTime Today => DateTime.Now.ToLocalTime();

        private bool isVisible = true;
        /// <summary>
        /// Gets or sets whether this action item is visible in the UI.
        /// </summary>
        public bool IsVisible
        {
            get => isVisible;
            set
            {
                if (isVisible == value) return;

                SetProperty(ref isVisible, value);
            }
        }

        private string GetStartLabel()
        {
            if (StartDate == null && StartTime == null)
                return "From Now";

            var parts = new List<string>();

            if (StartDate != null)
                parts.Add(StartDate.Value.ToString("MMM dd"));

            if (StartTime != null)
                parts.Add(StartTime.Value.ToString(@"hh\:mm"));

            return "From " + string.Join(" ", parts);
        }
        private string GetEndLabel()
        {
            if (EndDate == null && EndTime == null)
                return "Until Forever";

            var parts = new List<string>();

            if (EndDate != null)
                parts.Add(EndDate.Value.ToString("MMM dd"));

            if (EndTime != null)
                parts.Add(EndTime.Value.ToString(@"hh\:mm"));

            return "Until " + string.Join(" ", parts);
        }
        /// <summary>
        /// Gets a summary of the action including pumps and volume (e.g., "P1→P2 100µL").
        /// </summary>
        public string Summary => $"{GetPumpSummary()} {Amount}µL";
        private string GetPumpSummary() => Type switch
        {
            ActionType.Aspirate => $"{Pump1Label}",
            ActionType.Dispense => $"{Pump1Label}",
            ActionType.Exchange => $"{Pump1Label}→{Pump2Label}",
            _ => ""
        };

        /// <summary>
        /// Gets a display label showing the time period for the action (e.g., "From Now Until Forever").
        /// </summary>
        public string PeriodLabel => $"{GetStartLabel()} {GetEndLabel()}";

        /// <summary>
        /// Gets a display label showing the countdown or status (e.g., "01:30:45", "Pending", "Finished").
        /// </summary>
        public string TimeUntilNextRunLabel =>
            TimeUntilNextRun > TimeSpan.Zero
            ? TimeUntilNextRun.ToString(@"hh\:mm\:ss")
            : GetTimeTextLabel();

        private string GetTimeTextLabel()
        {
            DateTimeOffset now = DateTimeOffset.Now;

            DateTimeOffset? start = null;

            if (StartDate.HasValue && StartTime.HasValue)
            {
                start = StartDate.Value.Date + StartTime.Value;
            }

            if (start == null) return "Finished";

            return now < start ? "Pending" : "Finished";
        }

        private bool suppressValidation;

        /// <summary>
        /// Initializes a new instance of the ActionItem class with specified parameters.
        /// </summary>
        /// <param name="tempId">Temporary identifier before persistence.</param>
        /// <param name="type">Type of action.</param>
        /// <param name="pump1">Primary pump.</param>
        /// <param name="pump2">Secondary pump (for Exchange actions).</param>
        /// <param name="amount">Volume in microliters.</param>
        /// <param name="frequency">How often the action repeats.</param>
        /// <param name="unit">Time unit for frequency.</param>
        public ActionItem(int tempId, ActionType type, Pump pump1, Pump pump2, int amount, int frequency, TimeUnit unit)
        {
            TempId = tempId;
            Type = type;
            Pump1 = pump1;  
            Pump2 = pump2;
            Amount = amount;
            Frequency = frequency;
            TimeUnit = unit;
        }

        /// <summary>
        /// Initializes a new instance of the ActionItem class from a ScheduleAction model.
        /// Subscribes to model property changes for synchronization.
        /// </summary>
        /// <param name="model">The schedule action model to wrap.</param>
        public ActionItem(ScheduleAction model)
        {
            Model = model;
            Model.PropertyChanged += ActionModel_PropertyChanged;
            Override(model);
        }

        private void ActionModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Override(Model);
        }

        /// <summary>
        /// Overrides this ActionItem's properties with values from a ScheduleAction model.
        /// Converts epoch times to local date/time values and suppresses validation during the update.
        /// </summary>
        /// <param name="model">The ScheduleAction model to copy from.</param>
        public void Override(ScheduleAction model)
        {
            suppressValidation = true;

            Id = model.Id;
            Type = model.Type;
            Pump1 = model.Pump1;
            Pump2 = model.Pump2;
            Amount = model.Amount;
            Frequency = model.Frequency;
            TimeUnit = model.TimeUnit;
            StartEpoch = model.StartEpoch;
            EndEpoch = model.EndEpoch;
            LastRunEpoch = model.LastRunEpoch;
            StartTime = null;
            StartDate = null;
            EndTime = null;
            EndDate = null;

            if (StartEpoch != 0)
            {
                DateTimeOffset start = DateTimeOffset.FromUnixTimeSeconds(StartEpoch).LocalDateTime;

                StartDate = new DateTimeOffset(start.Date);
                StartTime = start.TimeOfDay;
            }

            if (EndEpoch != 0)
            {
                DateTimeOffset end = DateTimeOffset.FromUnixTimeSeconds(EndEpoch).LocalDateTime;


                EndDate = new DateTimeOffset(end.Date);
                EndTime = end.TimeOfDay;
            }

            suppressValidation = false;
        }

        /// <summary>
        /// Initializes a new instance of the ActionItem class by copying another ActionItem.
        /// </summary>
        /// <param name="other">The ActionItem to copy from.</param>
        public ActionItem(ActionItem other)
        {
            Override(other);
        }

        public void Override(ActionItem other)
        {
            suppressValidation = true;

            Model = other.Model;
            Id = other.Id;
            Type = other.Type;
            Pump1 = other.Pump1;
            Pump2 = other.Pump2;
            Amount = other.Amount;
            Frequency = other.Frequency;
            TimeUnit = other.TimeUnit;
            StartEpoch = other.StartEpoch;
            EndEpoch = other.EndEpoch;
            LastRunEpoch = other.LastRunEpoch;
            StartDate = other.StartDate;
            StartTime = other.StartTime;
            EndDate = other.EndDate;
            EndTime = other.EndTime;

            IsVisible = other.IsVisible;

            suppressValidation = false;
        }

        /// <summary>
        /// Converts the StartDate/StartTime and EndDate/EndTime properties to Unix epoch seconds.
        /// Updates StartEpoch and EndEpoch properties.
        /// </summary>
        public void ConvertToEpoch()
        {
            if (StartDate != null && StartTime != null)
            {
                DateTime startDateTime = StartDate.Value.Date + StartTime.Value;
                StartEpoch = new DateTimeOffset(startDateTime).ToUnixTimeSeconds();
            }
            if (EndDate != null && EndTime != null)
            {
                DateTime endDateTime = EndDate.Value.Date + EndTime.Value;
                EndEpoch = new DateTimeOffset(endDateTime).ToUnixTimeSeconds();
            }
        }

        /// <summary>
        /// Updates the TimeUntilNextRun property based on the current time and the action's schedule.
        /// Calculates the next execution time for recurring actions based on the last run time.
        /// </summary>
        public void UpdateCountdown()
        {
            int intervalSeconds = GetIntervalSeconds();

            DateTimeOffset now = DateTimeOffset.Now;
            DateTimeOffset nextRun;

            if (Frequency < 0)
            {
                // ONE-TIME ACTION
                nextRun = DateTimeOffset.FromUnixTimeSeconds(StartEpoch);
            }
            else
            {
                if (LastRunEpoch == 0)
                {
                    nextRun = DateTimeOffset.FromUnixTimeSeconds(StartEpoch);
                }
                else
                {
                    var lastRun = DateTimeOffset.FromUnixTimeSeconds(LastRunEpoch);
                    nextRun = lastRun.AddSeconds(intervalSeconds);
                }
            }

            TimeUntilNextRun = nextRun - now;

            if (TimeUntilNextRun < TimeSpan.Zero) TimeUntilNextRun = TimeSpan.Zero;

            OnPropertyChanged(nameof(TimeUntilNextRunLabel));
        }

        private int GetIntervalSeconds()
        {
            if (Frequency <= 0) return 0;

            return TimeUnit switch
            {
                TimeUnit.Minute => Frequency * 60,
                TimeUnit.Hour => Frequency * 60 * 60,
                TimeUnit.Day => Frequency * 24 * 60 * 60,
                _ => 0
            };
        }

    }
}
