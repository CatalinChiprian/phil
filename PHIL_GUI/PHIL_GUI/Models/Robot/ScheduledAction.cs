using CommunityToolkit.Mvvm.ComponentModel;

namespace PHIL_GUI.Models
{
    public enum ActionType
    {
        Aspirate,
        Dispense
    }
    public enum TimeUnit
    {
        Hour,
        Day
    }
    public class ScheduledAction : ObservableObject
    {
        public int Id { get; set; }

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

        private int pump;
        public int Pump
        {
            get => pump;
            set
            {
                if (value == pump) return;
                SetProperty(ref pump, value);
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

        private TimeUnit unit;
        public TimeUnit Unit
        {
            get => unit;
            set
            {
                if (value == unit) return;
                SetProperty(ref unit, value);
            }
        }

        private long startTime;
        public long StartTime
        {
            get => startTime;
            set
            {
                if (value == startTime) return;
                SetProperty(ref startTime, value);
            }
        }

        private long endTime;
        public long EndTime
        {
            get => endTime;
            set
            {
                if (value == endTime) return;
                SetProperty(ref endTime, value);
            }
        }

        public ScheduledAction(int id, ActionType type, int pump, int amount, int frequency, TimeUnit unit, long startTime, long endTime)
        {
            Id = id;
            Type = type;
            Pump = pump;
            Amount = amount;
            Frequency = frequency;
            Unit = unit;
            StartTime = startTime;
            EndTime = endTime;
        }
    }
}
