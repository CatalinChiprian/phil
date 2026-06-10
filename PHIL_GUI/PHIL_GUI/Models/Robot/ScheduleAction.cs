using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace PHIL_GUI.Models
{
    /// <summary>
    /// Defines the types of liquid handling actions that can be performed.
    /// </summary>
    public enum ActionType
    {
        /// <summary>Aspirate (draw in) liquid from a well.</summary>
        Aspirate,
        /// <summary>Dispense (expel) liquid into a well.</summary>
        Dispense,
        /// <summary>Exchange liquid between two pumps.</summary>
        Exchange
    }
    /// <summary>
    /// Defines the time units for action scheduling.
    /// </summary>
    public enum TimeUnit
    {
        /// <summary>Time unit in minutes.</summary>
        Minute,
        /// <summary>Time unit in hours.</summary>
        Hour,
        /// <summary>Time unit in days.</summary>
        Day
    }
    /// <summary>
    /// Defines the available pumps in the system.
    /// </summary>
    public enum Pump
    {
        /// <summary>No pump selected.</summary>
        None = -1,
        /// <summary>Pump 1.</summary>
        P1 = 1,
        /// <summary>Pump 2.</summary>
        P2 = 2,
        /// <summary>Pump 3.</summary>
        P3 = 3,
        /// <summary>Pump 4.</summary>
        P4 = 4
    }
    /// <summary>
    /// Represents a scheduled action model for liquid handling operations.
    /// This is the data model counterpart to ActionItem (view model).
    /// </summary>
    public class ScheduleAction : ObservableObject, IAction
    {
        private const int INVALID_ID = 0;

        private int id;
        /// <summary>
        /// Gets or sets the unique identifier for this scheduled action.
        /// </summary>
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
        /// <summary>
        /// Gets or sets the type of action (Aspirate, Dispense, or Exchange).
        /// </summary>
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
        /// <summary>
        /// Gets or sets the primary pump. For Exchange actions, this is the source pump.
        /// </summary>
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
        /// <summary>
        /// Gets or sets the secondary pump. For Exchange actions, this is the destination pump.
        /// </summary>
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
            }
        }

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
            }
        }

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

        /// <summary>
        /// Initializes a new instance of the ScheduleAction class with all parameters.
        /// </summary>
        /// <param name="id">Unique identifier.</param>
        /// <param name="type">Type of action.</param>
        /// <param name="pump1">Primary pump.</param>
        /// <param name="pump2">Secondary pump.</param>
        /// <param name="amount">Volume in microliters.</param>
        /// <param name="frequency">Repetition frequency.</param>
        /// <param name="unit">Time unit for frequency.</param>
        /// <param name="startTime">Start time in Unix epoch seconds.</param>
        /// <param name="endTime">End time in Unix epoch seconds.</param>
        /// <param name="lastRunTime">Last run time in Unix epoch seconds.</param>
        public ScheduleAction(int id, ActionType type, Pump pump1, Pump pump2, int amount, int frequency, TimeUnit unit, long startTime, long endTime, long lastRunTime)
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
            LastRunEpoch = lastRunTime;
        }

        /// <summary>
        /// Initializes a new instance of the ScheduleAction class from an ActionItem.
        /// </summary>
        /// <param name="actionItem">The ActionItem to convert from.</param>
        public ScheduleAction(ActionItem actionItem)
        {
            UpdateFromActionItem(actionItem);
        }

        /// <summary>
        /// Updates this ScheduleAction's properties from an ActionItem.
        /// Uses TempId if Id is invalid.
        /// </summary>
        /// <param name="actionItem">The ActionItem to copy from.</param>
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
