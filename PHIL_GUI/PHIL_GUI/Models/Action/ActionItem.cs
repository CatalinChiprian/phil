using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;

namespace PHIL_GUI.Models
{
    public class ActionItem : ObservableObject, IAction
    {
        public ScheduleAction Model { get; set; }
        private int id;
        public int Id
        {
            get => id;
            set
            {
                if (value == id) return;
                SetProperty(ref id, value);
            }
        }
        public int TempId { get; set; }

        private ActionType type;
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
        public IBrush ActionTypeTextColor => Application.Current.Resources[actionTypeTextColor] as IBrush;

        private Pump pump1;
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
        public string Pump1Label => GetPumpName(pump1);

        private Pump pump2;
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
        public string Pump2Label => Type == ActionType.Exchange ? GetPumpName(pump2) : "";
        private string GetPumpName(Pump pump) => pump == Pump.None ? "" : $"Pump {pump.ToString()}";

        private int amount;
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
        public int Frequency
        {
            get => frequency;
            set
            {
                if (value == frequency) return;
                SetProperty(ref frequency, value);
                OnPropertyChanged(nameof(MetaLabel));
            }
        }

        private string GetFrequencyLabel() => Frequency != -1 ? $"Every {Frequency} {GetTimeUnitLabel()}" : "";

        private TimeUnit timeUnit;
        public TimeUnit TimeUnit
        {
            get => timeUnit;
            set
            {
                if (value == timeUnit) return;
                SetProperty(ref timeUnit, value);
                OnPropertyChanged(nameof(MetaLabel));
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
        public long EndEpoch
        {
            get => endEpoch;
            set
            {
                if (value == endEpoch) return;
                SetProperty(ref endEpoch, value);
            }
        }

        private DateTimeOffset? startDate;
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

                OnPropertyChanged(nameof(MetaLabel));
            }
        }

        private TimeSpan? startTime;
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

                OnPropertyChanged(nameof(MetaLabel));
            }
        }

        private DateTimeOffset? endDate;
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
                OnPropertyChanged(nameof(MetaLabel));
            }
        }

        private TimeSpan? endTime;
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

                OnPropertyChanged(nameof(MetaLabel));
            }
        }

        public DateTime Today => DateTime.Now.ToLocalTime();

        private bool isVisible = true;
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

            return "Until " + string.Join (" ", parts);
        }
        public string Summary => $"{GetPumpSummary()} {Amount}µL";
        private string GetPumpSummary() => Type switch
        {
            ActionType.Aspirate => $"{Pump1Label}",
            ActionType.Dispense => $"{Pump1Label}",
            ActionType.Exchange => $"IN {Pump1Label} OUT {Pump2Label}",
            _ => ""
        };

        public string MetaLabel => $"{GetFrequencyLabel()} {GetStartLabel()} {GetEndLabel()}";

        private bool suppressValidation;

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
            StartDate = other.StartDate;
            StartTime = other.StartTime;
            EndDate = other.EndDate;
            EndTime = other.EndTime;
            IsVisible = other.IsVisible;

            suppressValidation = false;
        }

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
    }
}
