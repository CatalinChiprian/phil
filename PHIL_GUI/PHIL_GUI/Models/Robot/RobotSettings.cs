using CommunityToolkit.Mvvm.ComponentModel;

namespace PHIL_GUI.Models
{
    public enum MoveState
    {
        Idle,
        Moving,
        EmergencyStopped,
    }
    public class RobotSettings : ObservableObject
    {
        private MoveState state;
        public MoveState State
        {
            get => state;
            set => SetProperty(ref state, value);
        }

        private string microsteps;
        public string Microsteps
        {
            get => microsteps;
            set => SetProperty(ref microsteps, value);
        }

        private double stepSize;
        public double StepSize
        {
            get => stepSize;
            set => SetProperty(ref stepSize, value);
        }
    }
}
