using CommunityToolkit.Mvvm.ComponentModel;

namespace PHIL_GUI.Models
{
    public enum ActionType
    {
        Aspirate,
        Dispense,
        Exchange
    }
    public enum TimeUnit
    {
        Hour,
        Day
    }
    public enum Pump
    {
        P1 = 1,
        P2 = 2,
        P3 = 3,
        P4 = 4
    }
    public class ScheduleAction : ObservableObject, IAction
    {
        private const int INVALID_ID = 0;

        private int id;
        public int Id
        {
            get => id;
            set
            {
                if (id == value) return;
                SetProperty(ref id, value);
            }
        }

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

        public ScheduleAction(int id, ActionType type, Pump pump1, Pump pump2, int amount, int frequency, TimeUnit unit, long startTime, long endTime)
        {
            Id = id;
            Type = type;
            Pump1 = pump1;
            Pump2 = pump2;
            Amount = amount;
            Frequency = frequency;
            TimeUnit = unit;
            StartEpoch = startTime;
            EndEpoch = endTime;
        }

        public ScheduleAction(ActionItem actionItem)
        {
            UpdateFromActionItem(actionItem);
        }

        public void UpdateFromActionItem(ActionItem actionItem)
        {
            Id = actionItem.Id == INVALID_ID ? actionItem.TempId : actionItem.Id;
            Type = actionItem.Type;
            Pump1 = actionItem.Pump1;
            Pump2 = actionItem.Pump2;
            Amount = actionItem.Amount;
            Frequency = actionItem.Frequency;
            TimeUnit = actionItem.TimeUnit;
            StartEpoch = actionItem.StartEpoch;
            EndEpoch = actionItem.EndEpoch;
        }
    }
}
