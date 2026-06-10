using CommunityToolkit.Mvvm.ComponentModel;

namespace PHIL_GUI.Models
{
    /// <summary>
    /// Defines the movement states of the robot.
    /// </summary>
    public enum MoveState
    {
        /// <summary>Robot is not moving.</summary>
        Idle,
        /// <summary>Robot is currently moving.</summary>
        Moving,
        /// <summary>Robot has been emergency stopped.</summary>
        EmergencyStopped,
    }
    /// <summary>
    /// Represents the robot's configuration settings.
    /// </summary>
    public class RobotSettings : ObservableObject
    {
        private MoveState state;
        /// <summary>
        /// Gets or sets the current movement state of the robot.
        /// </summary>
        public MoveState State
        {
            get => state;
            set => SetProperty(ref state, value);
        }

        private string microsteps;
        /// <summary>
        /// Gets or sets the microsteps configuration for motor precision.
        /// </summary>
        public string Microsteps
        {
            get => microsteps;
            set => SetProperty(ref microsteps, value);
        }

        private double stepSize;
        /// <summary>
        /// Gets or sets the physical distance per motor step.
        /// </summary>
        public double StepSize
        {
            get => stepSize;
            set => SetProperty(ref stepSize, value);
        }
    }
}
