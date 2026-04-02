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

        private int x;
        public int X
        {
            get => x;
            set => SetProperty(ref x, value);
        }

        private int y;
        public int Y
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

        public string XY => $"{X}, {Y}";
        public string ErrL => ErrorLeft >= 0 ? $"{(ErrorLeft >= 0 ? "+" : "")}{ErrorLeft:F2}°" : "";
        public string ErrR => ErrorRight >= 0 ? $"{(ErrorRight >= 0 ? "+" : "")}{ErrorRight:F2}°" : "";

        string errLColor => ErrorLeft < 0 ? "Text" : ErrorLeft < 1 ? "Accent" : ErrorLeft < 2 ? "Caution" : "Warn";
        string errRColor => ErrorRight < 0 ? "Text" : ErrorRight < 1 ? "Accent" : ErrorRight < 2 ? "Caution" : "Warn";

        public IBrush ErrLColor => Application.Current.Resources[errLColor] as IBrush;
        public IBrush ErrRColor => Application.Current.Resources[errRColor] as IBrush;

        private bool HasError => ErrorLeft >= 0 && ErrorRight >= 0;
        public double WorstError => Math.Max(ErrorLeft, ErrorRight);
        public bool IsOk => HasError && WorstError < 1;
        public bool IsCaution => HasError && WorstError >= 1 && WorstError < 2;
        public bool IsWarn => HasError && WorstError >= 2;
        public bool IsNoRecord => !HasError;


        public double ErrLBarWidth => Math.Min(Math.Abs(ErrorLeft) / 3.0 * 40, 40);
        public double ErrRBarWidth => Math.Min(Math.Abs(ErrorRight) / 3.0 * 40, 40);
    }
}
