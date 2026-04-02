using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace PHIL_GUI.Models
{
    public class WellItem : ObservableObject
    {
        public char Row { get; }
        public int Column { get; }

        private string name;
        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set => SetProperty(ref isSelected, value);
        }

        private bool isVisible;
        public bool IsVisible
        {
            get => isVisible;
            set => SetProperty(ref isVisible, value);
        }
        
        private CalibrationPoint calibration;
        public CalibrationPoint Calibration
        {
            get => calibration;
            set
            {
                SetProperty(ref calibration, value);

                OnPropertyChanged(nameof(IsCalibrated));
            }
        }

        public bool IsCalibrated => Calibration != null;
        public WellItem(char row, int column, bool isVisible = true)
        {
            Row = row;
            Column = column;
            Name = $"{row}{column}";
            IsVisible = isVisible;
        }
    }
}
