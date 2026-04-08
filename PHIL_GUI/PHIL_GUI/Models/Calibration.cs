using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace PHIL_GUI.Models
{
    public class Calibration : ObservableObject
    {
        public const int MAX_COUNT_96 = 40;
        public const int MAX_COUNT_OOC = 32;
        public const int MIN_COUNT = 10;
        public int Count => Points.Count();
        public ObservableCollection<CalibrationPoint> Points { get; } = new ObservableCollection<CalibrationPoint>();

        private double? rmsL;
        public double? RmsL { 
            get => rmsL;
            set
            {
                SetProperty(ref rmsL, value);

                OnPropertyChanged(nameof(rmsL));
            }
        }

        private double? rmsR;
        public double? RmsR
        {
            get => rmsR;
            set
            {
                SetProperty(ref rmsR, value);

                OnPropertyChanged(nameof(RmsRText));
            }
        }

        public string RmsLText => RmsL.HasValue ? $"L {RmsL.Value:F2}°" : "L -";
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
        public IBrush RmsRColor => Application.Current.Resources[rmsRColor] as IBrush;

        public string RmsDisplayText => RmsL.HasValue && RmsR.HasValue ? $"L {RmsL:F2}°  R {RmsR:F2}°" : "L - R -";

        private string rmsColor
        {
            get
            {
                if (!RmsL.HasValue && !RmsR.HasValue) return "Muted"; 
                double worst = Math.Max(Math.Abs(RmsL!.Value), Math.Abs(RmsR!.Value));
                if (worst > 1.5) return "Warn";
                if (worst > 1.0) return "Caution";
                return "Accent";
            }
        }
        public IBrush RmsColor => Application.Current.Resources[rmsColor] as IBrush;

        private string pointsColor => Count < 10 ? "Warn" : Count < 20 ? "Caution" : "Accent";
        public IBrush PointsColor => Application.Current.Resources[pointsColor] as IBrush;

        public bool IsCalibrated => RmsL.HasValue && RmsR.HasValue;

        public Calibration()
        {
            Points.CollectionChanged += Points_CollectionChanged;
        }

        private void Points_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Count));
        }
    }
}
