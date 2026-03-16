using CommunityToolkit.Mvvm.ComponentModel;
using PHIL_GUI.Models;

namespace PHIL_GUI.Services
{
    public class RobotStateService : ObservableObject
    {
        public Settings Settings { get; } = new Settings();
        public Position Position { get; } = new Position();
        public LimitSwitches Limit { get; } = new LimitSwitches();
        public Calibration Calibration { get; } = new Calibration();
        public Well CurrentWell { get; } = new Well();

        private double rmsL, rmsR;
        public double RmsL { get => rmsL; set => SetProperty(ref rmsL, value); }
        public double RmsR { get => rmsR; set => SetProperty(ref rmsR, value); }
    }
}
