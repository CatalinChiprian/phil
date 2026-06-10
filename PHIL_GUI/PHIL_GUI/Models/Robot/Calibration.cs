using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace PHIL_GUI.Models
{
    /// <summary>
    /// Manages the robot's calibration data including calibration points and RMS error values.
    /// Tracks calibration quality with visual feedback colors.
    /// </summary>
    public class Calibration : ObservableObject
    {
        /// <summary>
        /// Maximum number of calibration points for organ-on-chip configuration.
        /// </summary>
        public const int MAX_COUNT_OOC = 32;
        /// <summary>
        /// Minimum number of calibration points required for valid calibration.
        /// </summary>
        public const int MIN_COUNT = 10;
        /// <summary>
        /// Gets the current number of calibration points.
        /// </summary>
        public int Count => Points.Count();
        /// <summary>
        /// Gets a display label showing current point count vs maximum.
        /// </summary>
        public string CalPointsText => $"{Count}/{(MAX_COUNT_OOC)}";
        /// <summary>
        /// Gets the collection of calibration points.
        /// </summary>
        public ObservableCollection<CalibrationPoint> Points { get; } = new ObservableCollection<CalibrationPoint>();

        private double? rmsL;
        /// <summary>
        /// Gets or sets the root mean square (RMS) error for the left motor in degrees.
        /// </summary>
        public double? RmsL {
            get => rmsL;
            set
            {
                SetProperty(ref rmsL, value);

                OnPropertyChanged(nameof(RmsLText));
                OnPropertyChanged(nameof(RmsLColor));
                OnPropertyChanged(nameof(RmsColor));
                OnPropertyChanged(nameof(IsCalibrated));
                OnPropertyChanged(nameof(RmsDisplayText));
            }
        }

        private double? rmsR;
        /// <summary>
        /// Gets or sets the root mean square (RMS) error for the right motor in degrees.
        /// </summary>
        public double? RmsR
        {
            get => rmsR;
            set
            {
                SetProperty(ref rmsR, value);

                OnPropertyChanged(nameof(RmsRText));
                OnPropertyChanged(nameof(RmsRColor));
                OnPropertyChanged(nameof(RmsColor));
                OnPropertyChanged(nameof(IsCalibrated));
                OnPropertyChanged(nameof(RmsDisplayText));
            }
        }

        /// <summary>
        /// Gets a display label for the left motor RMS error.
        /// </summary>
        public string RmsLText => RmsL.HasValue ? $"L {RmsL.Value:F2}°" : "L -";
        /// <summary>
        /// Gets a display label for the right motor RMS error.
        /// </summary>
        public string RmsRText => RmsR.HasValue ? $"L {RmsR.Value:F2}°" : "R -";

        private string rmsLColor
        {
            get
            {
                if (RmsL > 1.5) return "Warn";
                if (RmsL > 1.0) return "Caution";
                return "Accent";
            }
        }
        /// <summary>
        /// Gets the color brush for the left motor RMS display based on error level.
        /// </summary>
        public IBrush RmsLColor => Application.Current.Resources[rmsLColor] as IBrush;

        private string rmsRColor
        {
            get
            {
                if (RmsR > 1.5) return "Warn";
                if (RmsR > 1.0) return "Caution";
                return "Accent";
            }
        }
        /// <summary>
        /// Gets the color brush for the right motor RMS display based on error level.
        /// </summary>
        public IBrush RmsRColor => Application.Current.Resources[rmsRColor] as IBrush;

        /// <summary>
        /// Gets a combined display string showing both left and right RMS errors.
        /// </summary>
        public string RmsDisplayText => RmsL.HasValue && RmsR.HasValue ? $"L {RmsL:F2}°  R {RmsR:F2}°" : "L - R -";

        private string rmsColor
        {
            get
            {
                if (!RmsL.HasValue || !RmsR.HasValue) return "Muted"; 
                double worst = Math.Max(Math.Abs(RmsL.Value), Math.Abs(RmsR.Value));
                if (worst > 1.5) return "Warn";
                if (worst > 1.0) return "Caution";
                return "Accent";
            }
        }
        /// <summary>
        /// Gets the overall color brush based on the worst RMS error.
        /// </summary>
        public IBrush RmsColor => Application.Current.Resources[rmsColor] as IBrush;

        private string pointsColor => Count < 10 ? "Warn" : Count < 20 ? "Caution" : "Accent";
        /// <summary>
        /// Gets the color brush for the calibration point count display.
        /// </summary>
        public IBrush PointsColor => Application.Current.Resources[pointsColor] as IBrush;

        /// <summary>
        /// Gets whether the robot has valid calibration data (both RMS values present).
        /// </summary>
        public bool IsCalibrated => RmsL.HasValue && RmsR.HasValue;

        /// <summary>
        /// Initializes a new instance of the Calibration class and subscribes to point collection changes.
        /// </summary>
        public Calibration()
        {
            Points.CollectionChanged += Points_CollectionChanged;
        }

        private void Points_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Count));
            OnPropertyChanged(nameof(CalPointsText));
        }
    }
}
