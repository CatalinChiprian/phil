using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Linq;

namespace PHIL_GUI.Models
{
    public class Calibration : ObservableObject
    {
        public const int MAX_COUNT = 40;
        public int Count => Points.Count();
        public List<CalibrationPoint> Points { get; } = new List<CalibrationPoint>(MAX_COUNT);

        private string rmsL;
        public string RmsL { 
            get => rmsL;
            set => SetProperty(ref rmsL, value);
        }

        private string rmsR;
        public string RmsR 
        {
            get => rmsR;
            set => SetProperty(ref rmsR, value); 
        }
    }
}
