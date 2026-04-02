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
            set
            {
                if (value ==  x) return;

                SetProperty(ref x, value);

                OnPropertyChanged(nameof(XY));
            }
        }

        private int y;
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

        private double errorLeft;
        public double ErrorLeft
        {
            get => errorLeft;
            set
            {
                if (value == errorLeft) return;

                SetProperty(ref errorLeft, value);

                OnPropertyChanged(nameof(ErrL));
                OnPropertyChanged(nameof(ErrLColor));
                OnPropertyChanged(nameof(ErrLBarWidth));

                OnPropertyChanged(nameof(IsOk));
                OnPropertyChanged(nameof(IsCaution));
                OnPropertyChanged(nameof(IsWarn));
            }
        }

        private double errorRight;
        public double ErrorRight
        {
            get => errorRight;
            set
            {
                if (value == errorRight) return;

                SetProperty(ref errorRight, value);

                OnPropertyChanged(nameof(ErrR));
                OnPropertyChanged(nameof(ErrRColor));
                OnPropertyChanged(nameof(ErrRBarWidth));

                OnPropertyChanged(nameof(IsOk));
                OnPropertyChanged(nameof(IsCaution));
                OnPropertyChanged(nameof(IsWarn));
            }
        }

        public string XY => $"{X}, {Y}";
        public string ErrL => ErrorLeft >= 0 ? $"{(ErrorLeft >= 0 ? "+" : "")}{ErrorLeft:F2}°" : "";
        public string ErrR => ErrorRight >= 0 ? $"{(ErrorRight >= 0 ? "+" : "")}{ErrorRight:F2}°" : "";

        string errLColor => ErrorLeft < 0 ? "Muted" : ErrorLeft < 1 ? "Accent" : ErrorLeft < 2 ? "Caution" : "Warn";
        string errRColor => ErrorRight < 0 ? "Muted" : ErrorRight < 1 ? "Accent" : ErrorRight < 2 ? "Caution" : "Warn";

        public IBrush ErrLColor => Application.Current.Resources[errLColor] as IBrush;
        public IBrush ErrRColor => Application.Current.Resources[errRColor] as IBrush;

        private bool HasError => ErrorLeft >= 0 && ErrorRight >= 0;
        public double WorstError => Math.Max(ErrorLeft, ErrorRight);
        public bool IsOk => HasError && WorstError < 1;
        public bool IsCaution => HasError && WorstError >= 1 && WorstError < 2;
        public bool IsWarn => HasError && WorstError >= 2;


        public double ErrLBarWidth => Math.Min(Math.Abs(ErrorLeft) / 3.0 * 40, 40);
        public double ErrRBarWidth => Math.Min(Math.Abs(ErrorRight) / 3.0 * 40, 40);

        public CalibrationPoint(string name, int x = 0, int y = 0, double errorLeft = -2, double errorRight = -2)
        {
            Name = name;
            X = x;
            Y = y;
            ErrorLeft = errorLeft;
            ErrorRight = errorRight;
        }
    }
}
