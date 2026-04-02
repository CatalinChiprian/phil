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

        private double rmsLValue;
        public double RmsLValue { 
            get => rmsLValue;
            set => SetProperty(ref rmsLValue, value);
        }

        private double rmsRValue;
        public double RmsRValue
        {
            get => rmsRValue;
            set => SetProperty(ref rmsRValue, value); 
        }

        public string RmsDisplayText => $"L {RmsLValue:F2}°  R {RmsRValue:F2}°";

        private string rmsColor
        {
            get
            {
                double worst = Math.Max(RmsLValue, RmsRValue);
                if (worst > 1.5) return "Warn";
                if (worst > 1.0) return "Caution";
                return "Accent";
            }
        }
        public IBrush RmsColor => Application.Current.Resources[rmsColor] as IBrush;

        private string pointsColor => Count < 10 ? "Warn" : Count < 20 ? "Caution" : "Accent";
        public IBrush PointsColor => Application.Current.Resources[pointsColor] as IBrush;

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
