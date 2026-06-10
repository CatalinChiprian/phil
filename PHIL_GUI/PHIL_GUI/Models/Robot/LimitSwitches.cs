using CommunityToolkit.Mvvm.ComponentModel;

namespace PHIL_GUI.Models
{
    /// <summary>
    /// Represents the state of the robot's limit switches.
    /// Limit switches detect when motors reach their physical boundaries.
    /// </summary>
    public class LimitSwitches : ObservableObject
    {
        private bool z1;
        /// <summary>
        /// Gets or sets whether the first Z-axis limit switch is triggered.
        /// </summary>
        public bool Z1
        { 
            get => z1;
            set
            {
                SetProperty(ref z1, value);
                OnPropertyChanged(nameof(CanMoveUp));
            }
        }

        private bool z2;
        /// <summary>
        /// Gets or sets whether the second Z-axis limit switch is triggered.
        /// </summary>
        public bool Z2
        { 
            get => z2;
            set
            {
                SetProperty(ref z2, value);
                OnPropertyChanged(nameof(CanMoveUp));
            }
        }

        private bool l;
        /// <summary>
        /// Gets or sets whether the left motor limit switch is triggered.
        /// </summary>
        public bool L 
        { 
            get => l; 
            set => SetProperty(ref l, value);
        }

        private bool r;
        /// <summary>
        /// Gets or sets whether the right motor limit switch is triggered.
        /// </summary>
        public bool R 
        { 
            get => r; 
            set => SetProperty(ref r, value);
        }

        /// <summary>
        /// Gets whether the robot can safely move upward.
        /// Returns false if either Z-axis limit switch is triggered.
        /// </summary>
        public bool CanMoveUp => !(Z1 || Z2);
    }
}
