using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHIL_GUI.Models
{
    public class ActionItem : ObservableObject
    {
        public int Id { get; set; }
        public int TempId { get; set; }

        private ActionType type;
        public ActionType Type
        {
            get => type;
            set
            {
                if (value == type) return;
                SetProperty(ref type, value);
            }
        }

        private Pump pump1;
        public Pump Pump1
        {
            get => pump1;
            set
            {
                if (value == pump1) return;
                SetProperty(ref pump1, value);
            }
        }

        private Pump pump2;
        public Pump Pump2
        {
            get => pump2;
            set
            {
                if (value == pump2) return;
                SetProperty(ref pump2, value);
            }
        }

        private int amount;
        public int Amount
        {
            get => amount;
            set
            {
                if (value == amount) return;
                SetProperty(ref amount, value);
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
            }
        }

        private TimeUnit timeUnit;
        public TimeUnit TimeUnit
        {
            get => timeUnit;
            set
            {
                if (value == timeUnit) return;
                SetProperty(ref timeUnit, value);
            }
        }

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

                if (value < Today.Date) value = Today.Date;
                if (startDate == null && startTime == null) SetProperty(ref startTime, Today.TimeOfDay, nameof(StartTime));

                // Since DatetimePicker has LocalValue priority the UI might still receive the incorrect value, so we need to reset it to null first to ensure the correct value is displayed
                if (Equals(startDate, value)) SetProperty(ref startDate, null);

                SetProperty(ref startDate, value);

                if (EndDate.HasValue && value > EndDate.Value.Date) EndDate = value;
            }
        }

        private TimeSpan? startTime;
        public TimeSpan? StartTime
        {
            get => startTime;
            set
            {
                if (value == startTime) return;

                if (StartDate?.Date == Today.Date && value < Today.TimeOfDay) value = Today.TimeOfDay;

                // Since DatetimePicker has LocalValue priority the UI might still receive the incorrect value, so we need to reset it to null first to ensure the correct value is displayed
                if (Equals(startTime, value)) SetProperty(ref startTime, null);

                SetProperty(ref startTime, value);

                if (startDate == null) StartDate = Today.Date;
            }
        }

        private DateTimeOffset? endDate;
        public DateTimeOffset? EndDate
        {
            get => endDate;
            set
            {
                if (value == endDate) return;

                if (value < StartDate?.Date || value < Today.Date) value = StartDate?.Date ?? Today.Date;
                if (endDate == null && endTime == null) SetProperty(ref endTime, Today.TimeOfDay, nameof(endTime));

                // Since DatetimePicker has LocalValue priority the UI might still receive the incorrect value, so we need to reset it to null first to ensure the correct value is displayed
                if (Equals(endDate, value)) SetProperty(ref endDate, null);

                SetProperty(ref endDate, value);
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
                if (value < Today.TimeOfDay) value = Today.TimeOfDay;

                // Since DatetimePicker has LocalValue priority the UI might still receive the incorrect value, so we need to reset it to null first to ensure the correct value is displayed
                if (Equals(endTime, value)) SetProperty(ref endTime, null);

                SetProperty(ref endTime, value);

                if (endDate == null) EndDate = Today;
            }
        }

        public DateTime Today => DateTime.Now.ToLocalTime();

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

        public ActionItem(ScheduledAction other)
        {
            Id = other.Id;
            Type = other.Type;
            Pump1 = other.Pump1;
            Pump2 = other.Pump2;
            Amount = other.Amount;
            Frequency = other.Frequency;
            TimeUnit = other.TimeUnit;
            StartEpoch = other.StartEpoch;
            EndEpoch = other.EndEpoch;

            DateTimeOffset start = DateTimeOffset.FromUnixTimeSeconds(StartEpoch).LocalDateTime;
            DateTimeOffset end = DateTimeOffset.FromUnixTimeSeconds(EndEpoch).LocalDateTime;

            StartDate = new DateTimeOffset(start.Date);
            StartTime = start.TimeOfDay;

            EndDate = new DateTimeOffset(end.Date);
            EndTime = end.TimeOfDay;
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
