namespace PHIL_GUI.Models
{
    public class RobotState
    {
        public Settings Settings { get; } = new Settings();
        public LimitSwitches Limit { get; } = new LimitSwitches();
        public Calibration Calibration { get; } = new Calibration();
        public Position Position { get; } = new Position();
        public Well CurrentWell { get; } = new Well();
    }
}
