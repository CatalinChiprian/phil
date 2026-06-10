using CommunityToolkit.Mvvm.ComponentModel;

namespace PHIL_GUI.Models
{
    /// <summary>
    /// Defines the types of well positions the robot can recognize.
    /// </summary>
    public enum WellType
    {
        /// <summary>The home position of the robot.</summary>
        Home,
        /// <summary>A standard well in the plate.</summary>
        Standard,
        /// <summary>Position type is unknown.</summary>
        Unknown,
        /// <summary>A container position (not a well).</summary>
        Container
    }
    /// <summary>
    /// Represents a well or position that the robot can navigate to.
    /// Includes position coordinates and angle calibration data.
    /// </summary>
    public class Well : ObservableObject
    {
        private WellType type;
        /// <summary>
        /// Gets or sets the type of well.
        /// </summary>
        public WellType Type
        {
            get => type;
            set
            {
                if (value == type) return;

                SetProperty(ref type, value);
                if (type == WellType.Home) Name = "Home";
                if (type == WellType.Unknown) Name = "Unknown";
                OnPropertyChanged(nameof(IsStandard));
            }
        }

        /// <summary>
        /// Gets whether this well is a standard well type.
        /// </summary>
        public bool IsStandard => Type == WellType.Standard;

        private string name;
        /// <summary>
        /// Gets or sets the name of the well (e.g., "A1", "Home").
        /// </summary>
        public string Name
        {
            get => name;
            set
            {
                if (value == name) return;

                SetProperty(ref name, value);
            }
        }

        private double x;
        /// <summary>
        /// Gets or sets the X-coordinate of the well position.
        /// </summary>
        public double X
        {
            get => x;
            set => SetProperty(ref x, value);
        }

        private double y;
        /// <summary>
        /// Gets or sets the Y-coordinate of the well position.
        /// </summary>
        public double Y
        {
            get => y;
            set => SetProperty(ref y, value);
        }

        private string angleL;
        /// <summary>
        /// Gets or sets the left motor angle for this well position.
        /// </summary>
        public string AngleL
        {
            get => angleL;
            set
            {
                SetProperty(ref angleL, value);

                OnPropertyChanged(nameof(Angles));
            }
        }

        private string angleR;
        /// <summary>
        /// Gets or sets the right motor angle for this well position.
        /// </summary>
        public string AngleR
        {
            get => angleR;
            set
            {
                SetProperty(ref angleR, value);

                OnPropertyChanged(nameof(Angles));
            }
        }

        /// <summary>
        /// Gets a formatted string displaying both left and right angles.
        /// </summary>
        public string Angles => $"{AngleL}° / {AngleR}°";

        /// <summary>
        /// Initializes a new instance of the Well class with Name set to "Home".
        /// </summary>
        public Well()
        {
            Name = "Home";
        }
    }
}
