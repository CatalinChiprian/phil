using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace PHIL_GUI.Models
{
    /// <summary>
    /// Represents a single calibration point in the robot's calibration map.
    /// Stores position coordinates and angular error measurements for both left and right motors.
    /// </summary>
    public class CalibrationPoint : ObservableObject
    {
        private string name;
        /// <summary>
        /// Gets or sets the name identifier for this calibration point.
        /// </summary>
        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        private int x;
        /// <summary>
        /// Gets or sets the X-coordinate of the calibration point.
        /// </summary>
        public int X
        {
            get => x;
            set
            {
                if (value ==  x) return;

                SetProperty(ref x, value);

                OnPropertyChanged(nameof(XY));
            }
        }

        private int y;
        /// <summary>
        /// Gets or sets the Y-coordinate of the calibration point.
        /// </summary>
        public int Y
        {
            get => y;
            set
            {
                if (value == y) return;

                SetProperty(ref y, value);

                OnPropertyChanged(nameof(XY));
            }
        }

        /// <summary>
        /// Gets a formatted string displaying the X,Y coordinates.
        /// </summary>
        public string XY => $"{X}, {Y}";

        private double? errorLeft;
        /// <summary>
        /// Gets or sets the signed angular error for the left motor in degrees.
        /// Positive indicates clockwise error, negative indicates counter-clockwise.
        /// </summary>
        public double? ErrorLeft
        {
            get => errorLeft;
            set
            {
                if (value == errorLeft) return;

                SetProperty(ref errorLeft, value);

                AbsErrorLeft = errorLeft.HasValue ? Math.Abs(errorLeft.Value) : null;
            }
        }

        private double? absErrorLeft;
        /// <summary>
        /// Gets or sets the absolute angular error for the left motor in degrees.
        /// Automatically updated when ErrorLeft changes.
        /// </summary>
        public double? AbsErrorLeft
        {
            get => absErrorLeft;
            set
            {
                if (value == absErrorLeft) return;

                SetProperty(ref absErrorLeft, value);

                OnPropertyChanged(nameof(ErrL));
                OnPropertyChanged(nameof(ErrLColor));
                OnPropertyChanged(nameof(ErrLBarWidth));

                OnPropertyChanged(nameof(IsSolved));
                OnPropertyChanged(nameof(IsRecorded));
                OnPropertyChanged(nameof(WorstError));

                OnPropertyChanged(nameof(IsOk));
                OnPropertyChanged(nameof(IsCaution));
                OnPropertyChanged(nameof(IsWarn));
            }
        }

        private double? errorRight;
        /// <summary>
        /// Gets or sets the signed angular error for the right motor in degrees.
        /// Positive indicates clockwise error, negative indicates counter-clockwise.
        /// </summary>
        public double? ErrorRight
        {
            get => errorRight;
            set
            {
                if (value == errorRight) return;

                SetProperty(ref errorRight, value);

                AbsErrorRight = errorRight.HasValue ? Math.Abs(errorRight!.Value) : null;
            }
        }

        private double? absErrorRight;
        /// <summary>
        /// Gets or sets the absolute angular error for the right motor in degrees.
        /// Automatically updated when ErrorRight changes.
        /// </summary>
        public double? AbsErrorRight
        {
            get => absErrorRight;
            set
            {
                if (value == absErrorRight) return;

                SetProperty(ref absErrorRight, value);

                OnPropertyChanged(nameof(ErrR));
                OnPropertyChanged(nameof(ErrRColor));
                OnPropertyChanged(nameof(ErrRBarWidth));

                OnPropertyChanged(nameof(IsSolved));
                OnPropertyChanged(nameof(IsRecorded));
                OnPropertyChanged(nameof(WorstError));

                OnPropertyChanged(nameof(IsOk));
                OnPropertyChanged(nameof(IsCaution));
                OnPropertyChanged(nameof(IsWarn));
            }
        }


        /// <summary>
        /// Gets a formatted display string for the left error value.
        /// </summary>
        public string ErrL => ErrorLeft.HasValue ? $"{ErrorLeft.Value:F2}°" : "";
        /// <summary>
        /// Gets a formatted display string for the right error value.
        /// </summary>
        public string ErrR => ErrorRight.HasValue ? $"{ErrorRight.Value:F2}°" : "";

        string errLColor => !AbsErrorLeft.HasValue ? "Muted" : AbsErrorLeft!.Value < 1 ? "Accent" : AbsErrorLeft!.Value < 2 ? "Caution" : "Warn";
        string errRColor => !AbsErrorRight.HasValue ? "Muted" : AbsErrorRight!.Value < 1 ? "Accent" : AbsErrorRight!.Value < 2 ? "Caution" : "Warn";

        /// <summary>
        /// Gets the color brush for the left error display based on error magnitude.
        /// </summary>
        public IBrush ErrLColor => Application.Current.Resources[errLColor] as IBrush;
        /// <summary>
        /// Gets the color brush for the right error display based on error magnitude.
        /// </summary>
        public IBrush ErrRColor => Application.Current.Resources[errRColor] as IBrush;

        /// <summary>
        /// Gets whether both left and right errors have been calculated.
        /// </summary>
        public bool IsSolved => AbsErrorLeft.HasValue && AbsErrorRight.HasValue;
        /// <summary>
        /// Gets whether this point has been recorded but not yet solved.
        /// </summary>
        public bool IsRecorded => !IsSolved;
        /// <summary>
        /// Gets the larger of the two absolute errors.
        /// </summary>
        public double WorstError => Math.Max(AbsErrorLeft!.Value, AbsErrorRight!.Value);
        /// <summary>
        /// Gets whether the point is solved with errors less than 1 degree.
        /// </summary>
        public bool IsOk => IsSolved && WorstError < 1;
        /// <summary>
        /// Gets whether the point has errors between 1 and 2 degrees (caution level).
        /// </summary>
        public bool IsCaution => IsSolved && WorstError >= 1 && WorstError < 2;
        /// <summary>
        /// Gets whether the point has errors of 2 degrees or more (warning level).
        /// </summary>
        public bool IsWarn => IsSolved && WorstError >= 2;


        /// <summary>
        /// Gets the visual bar width for the left error display (scaled 0-40 based on error).
        /// </summary>
        public double ErrLBarWidth => Math.Min(AbsErrorLeft.GetValueOrDefault() / 3.0 * 40, 40);
        /// <summary>
        /// Gets the visual bar width for the right error display (scaled 0-40 based on error).
        /// </summary>
        public double ErrRBarWidth => Math.Min(AbsErrorRight.GetValueOrDefault() / 3.0 * 40, 40);

        /// <summary>
        /// Initializes a new instance of the CalibrationPoint class.
        /// </summary>
        /// <param name="name">The name identifier for this point.</param>
        /// <param name="x">The X-coordinate.</param>
        /// <param name="y">The Y-coordinate.</param>
        /// <param name="errorLeft">Optional left motor angular error.</param>
        /// <param name="errorRight">Optional right motor angular error.</param>
        public CalibrationPoint(string name, int x = 0, int y = 0, double? errorLeft = null, double? errorRight = null)
        {
            Name = name;
            X = x;
            Y = y;
            ErrorLeft = errorLeft;
            ErrorRight = errorRight;
        }
    }
}
