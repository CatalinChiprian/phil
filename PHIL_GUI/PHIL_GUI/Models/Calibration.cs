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
    }
}
