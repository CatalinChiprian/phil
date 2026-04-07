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

        private double? errorLeft;
        public double? ErrorLeft
        {
            get => errorLeft;
            set
            {
                if (value == errorLeft) return;

                SetProperty(ref errorLeft, value);

                if (!value.HasValue) return;

                AbsErrorLeft = Math.Abs(errorLeft!.Value);
            }
        }

        private double? absErrorLeft;
        public double? AbsErrorLeft
        {
            get => absErrorLeft;
            set
            {
                if (value == absErrorLeft) return;

                SetProperty(ref absErrorLeft, value);

                OnPropertyChanged(nameof(ErrL));
                OnPropertyChanged(nameof(ErrLColor));
                OnPropertyChanged(nameof(ErrLBarWidth));

                OnPropertyChanged(nameof(IsSolved));
                OnPropertyChanged(nameof(IsRecorded));
                OnPropertyChanged(nameof(WorstError));

                OnPropertyChanged(nameof(IsOk));
                OnPropertyChanged(nameof(IsCaution));
                OnPropertyChanged(nameof(IsWarn));
            }
        }

        private double? errorRight;
        public double? ErrorRight
        {
            get => errorRight;
            set
            {
                if (value == errorRight) return;

                SetProperty(ref errorRight, value);

                if (!value.HasValue) return;

                AbsErrorRight = Math.Abs(errorRight!.Value);
            }
        }

        private double? absErrorRight;
        public double? AbsErrorRight
        {
            get => absErrorRight;
            set
            {
                if (value == absErrorRight) return;

                SetProperty(ref absErrorRight, value);

                OnPropertyChanged(nameof(ErrR));
                OnPropertyChanged(nameof(ErrRColor));
                OnPropertyChanged(nameof(ErrRBarWidth));

                OnPropertyChanged(nameof(IsSolved));
                OnPropertyChanged(nameof(IsRecorded));
                OnPropertyChanged(nameof(WorstError));

                OnPropertyChanged(nameof(IsOk));
                OnPropertyChanged(nameof(IsCaution));
                OnPropertyChanged(nameof(IsWarn));
            }
        }

        public string XY => $"{X}, {Y}";
        public string ErrL => ErrorLeft.HasValue ? $"{ErrorLeft.Value:F2}°" : "";
        public string ErrR => ErrorRight.HasValue ? $"{ErrorRight.Value:F2}°" : "";

        string errLColor => !AbsErrorLeft.HasValue ? "Muted" : AbsErrorLeft!.Value < 1 ? "Accent" : AbsErrorLeft!.Value < 2 ? "Caution" : "Warn";
        string errRColor => !AbsErrorRight.HasValue ? "Muted" : AbsErrorRight!.Value < 1 ? "Accent" : AbsErrorRight!.Value < 2 ? "Caution" : "Warn";

        public IBrush ErrLColor => Application.Current.Resources[errLColor] as IBrush;
        public IBrush ErrRColor => Application.Current.Resources[errRColor] as IBrush;

        public bool IsSolved => AbsErrorLeft.HasValue && AbsErrorRight.HasValue;
        public bool IsRecorded => !IsSolved;
        public double WorstError => Math.Max(AbsErrorLeft!.Value, AbsErrorRight!.Value);
        public bool IsOk => IsSolved && WorstError < 1;
        public bool IsCaution => IsSolved && WorstError >= 1 && WorstError < 2;
        public bool IsWarn => IsSolved && WorstError >= 2;


        public double ErrLBarWidth => Math.Min(AbsErrorLeft.GetValueOrDefault() / 3.0 * 40, 40);
        public double ErrRBarWidth => Math.Min(AbsErrorRight.GetValueOrDefault() / 3.0 * 40, 40);

        public CalibrationPoint(string name, int x = 0, int y = 0, double? errorLeft = null, double? errorRight = null)
        {
            Name = name;
            X = x;
            Y = y;
            ErrorLeft = errorLeft;
            ErrorRight = errorRight;
        }
    }
}
