namespace PHIL_GUI.Models
{
    public class RobotState
    {
        public RobotSettings Settings { get; } = new RobotSettings();
        public LimitSwitches Limit { get; } = new LimitSwitches();
        public Calibration Calibration { get; } = new Calibration();
        public Position Position { get; } = new Position();
        public Well CurrentWell { get; } = new Well();
        public ActionScheduler ActionScheduler { get; } = new ActionScheduler();
    }
}
