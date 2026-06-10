namespace PHIL_GUI.Models
{
    /// <summary>
    /// Defines the contract for action items in the robot system.
    /// Implemented by both ActionItem (view model) and ScheduleAction (data model).
    /// </summary>
    public interface IAction
    {
        /// <summary>
        /// Gets or sets the unique identifier for this action.
        /// </summary>
        int Id { get; set; }
        /// <summary>
        /// Gets or sets the type of action (Aspirate, Dispense, or Exchange).
        /// </summary>
        ActionType Type { get; set; }
        /// <summary>
        /// Gets or sets the primary pump.
        /// </summary>
        Pump Pump1 { get; set; }
        /// <summary>
        /// Gets or sets the secondary pump (for Exchange actions).
        /// </summary>
        Pump Pump2 { get; set; }
        /// <summary>
        /// Gets or sets the liquid volume amount in microliters.
        /// </summary>
        int Amount { get; set; }
        /// <summary>
        /// Gets or sets the repetition frequency (-1 for one-time actions).
        /// </summary>
        int Frequency { get; set; }
        /// <summary>
        /// Gets or sets the time unit for the frequency.
        /// </summary>
        TimeUnit TimeUnit { get; set; }
        /// <summary>
        /// Gets or sets the start time as Unix epoch seconds.
        /// </summary>
        long StartEpoch { get; set; }
        /// <summary>
        /// Gets or sets the end time as Unix epoch seconds.
        /// </summary>
        long EndEpoch { get; set; }
    }
}
