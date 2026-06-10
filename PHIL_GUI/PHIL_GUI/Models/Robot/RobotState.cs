namespace PHIL_GUI.Models
{
    /// <summary>
    /// Represents the complete state of the robot system.
    /// Aggregates all robot subsystems including settings, limits, calibration, position, current well, and action scheduler.
    /// </summary>
    public class RobotState
    {
        /// <summary>
        /// Gets the robot's configuration settings.
        /// </summary>
        public RobotSettings Settings { get; } = new RobotSettings();
        /// <summary>
        /// Gets the state of the robot's limit switches.
        /// </summary>
        public LimitSwitches Limit { get; } = new LimitSwitches();
        /// <summary>
        /// Gets the robot's calibration data.
        /// </summary>
        public Calibration Calibration { get; } = new Calibration();
        /// <summary>
        /// Gets the robot's current motor positions.
        /// </summary>
        public Position Position { get; } = new Position();
        /// <summary>
        /// Gets information about the well the robot is currently positioned at.
        /// </summary>
        public Well CurrentWell { get; } = new Well();
        /// <summary>
        /// Gets the action scheduler for managing scheduled operations.
        /// </summary>
        public ActionScheduler ActionScheduler { get; } = new ActionScheduler();
    }
}
