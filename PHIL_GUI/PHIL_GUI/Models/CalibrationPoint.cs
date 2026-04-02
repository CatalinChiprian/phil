using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace PHIL_GUI.Models
{
    public class CalibrationPoint : ObservableObject
    {
        private string name;
        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        private string x;
        public string X
        {
            get => x;
            set => SetProperty(ref x, value);
        }

        private string y;
        public string Y
        {
            get => y;
            set => SetProperty(ref y, value);
        }

        private double errorLeft;
        public double ErrorLeft
        {
            get => errorLeft;
            set => SetProperty(ref errorLeft, value);
        }

        private double errorRight;
        public double ErrorRight
        {
            get => errorRight;
            set => SetProperty(ref errorRight, value);
        }

        public string XY => $"({X}, {Y})";
        public string ErrL => $"{(ErrorLeft >= 0 ? "+" : "")}{ErrorLeft:F2}°";
        public string ErrR => $"{(ErrorRight >= 0 ? "+" : "")}{ErrorRight:F2}°";

        string errLColor => Math.Abs(ErrorLeft) < 1 ? "Accent" : Math.Abs(ErrorLeft) < 2 ? "Caution" : "Warn";
        string errRColor => Math.Abs(ErrorRight) < 1 ? "Accent" : Math.Abs(ErrorRight) < 2 ? "Caution" : "Warn";

        public IBrush ErrLColor => Application.Current.Resources[errLColor] as IBrush;
        public IBrush ErrRColor => Application.Current.Resources[errRColor] as IBrush;
        public bool IsOk => Math.Max(Math.Abs(ErrorLeft), Math.Abs(ErrorRight)) < 1;
        public bool IsCaution => Math.Max(Math.Abs(ErrorLeft), Math.Abs(ErrorRight)) is >= 1 and < 2;
        public bool IsWarn => Math.Max(Math.Abs(ErrorLeft), Math.Abs(ErrorRight)) >= 2;
        public bool IsNoRecord => !IsOk && !IsCaution && !IsWarn;

        public double ErrLBarWidth => Math.Min(Math.Abs(ErrorLeft) / 3.0 * 40, 40);
        public double ErrRBarWidth => Math.Min(Math.Abs(ErrorRight) / 3.0 * 40, 40);
    }
}
