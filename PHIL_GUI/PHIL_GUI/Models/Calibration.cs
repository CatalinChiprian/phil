using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PHIL_GUI.Models
{
    public class Calibration : ObservableObject
    {
        public const int MAX_COUNT = 40;
        public const int MIN_COUNT = 10;
        public int Count => Points.Count();
        public List<CalibrationPoint> Points { get; } = new List<CalibrationPoint>(MAX_COUNT);

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

        public string CalPointsText => $"{Count}/{MAX_COUNT}";
        private string pointsColor => Count < 10 ? "Warn" : Count < 20 ? "Caution" : "Accent";
        public IBrush PointsColor => Application.Current.Resources[pointsColor] as IBrush;
    }
}
