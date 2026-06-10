using CommunityToolkit.Mvvm.ComponentModel;

namespace PHIL_GUI.Models
{
    /// <summary>
    /// Represents the current position of the robot's motors.
    /// Tracks left, right, and two Z-axis positions.
    /// </summary>
    public class Position : ObservableObject
    {
        private string l;
        /// <summary>
        /// Gets or sets the left motor position as a string value.
        /// </summary>
        public string L
        {
            get => l; 
            set => SetProperty(ref l, value);
        }

        private string r;
        /// <summary>
        /// Gets or sets the right motor position as a string value.
        /// </summary>
        public string R
        {
            get => r; 
            set => SetProperty(ref r, value);
        }

        private string z1;
        /// <summary>
        /// Gets or sets the first Z-axis motor position as a string value.
        /// </summary>
        public string Z1
        {
            get => z1;
            set => SetProperty(ref z1, value);
        }

        private string z2;
        /// <summary>
        /// Gets or sets the second Z-axis motor position as a string value.
        /// </summary>
        public string Z2
        {
            get => z2;
            set => SetProperty(ref z2, value);
        }

        /// <summary>
        /// Initializes a new instance of the Position class with all positions set to "0".
        /// </summary>
        public Position()
        {
            L = "0";
            R = "0";
            Z1 = "0";
            Z2 = "0";
        }
    }
}
