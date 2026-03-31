using Avalonia;
using Avalonia.Media;
using System;

namespace PHIL_GUI.Models
{
    public class CalibrationRowItem
    {
        public string Well { get; set; }
        public string XY { get; set; }
        public float ErrLValue { get; set; }
        public float ErrRValue { get; set; }

        public string ErrL => $"{(ErrLValue >= 0 ? "+" : "")}{ErrLValue:F2}°";
        public string ErrR => $"{(ErrRValue >= 0 ? "+" : "")}{ErrRValue:F2}°";

        string errLColor => Math.Abs(ErrLValue) < 1 ? "Accent" : Math.Abs(ErrLValue) < 2 ? "Caution" : "Warn";
        string errRColor => Math.Abs(ErrRValue) < 1 ? "Accent" : Math.Abs(ErrRValue) < 2 ? "Caution" : "Warn";

        public IBrush ErrLColor => Application.Current.Resources[errLColor] as IBrush;
        public IBrush ErrRColor => Application.Current.Resources[errRColor] as IBrush;

        public double ErrLBarWidth => Math.Min(Math.Abs(ErrLValue) / 3.0 * 40, 40);
        public double ErrRBarWidth => Math.Min(Math.Abs(ErrRValue) / 3.0 * 40, 40);

        public CalibrationRowItem(string well, string xy, float errL, float errR)
        {
            Well = well;
            XY = xy;
            ErrLValue = errL;
            ErrRValue = errR;
        }
    }
}
