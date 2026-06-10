using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace PHIL_GUI.Models
{
    /// <summary>
    /// Represents a single well in a well plate.
    /// Tracks selection state, visibility, and associated calibration data.
    /// </summary>
    public class WellItem : ObservableObject
    {
        /// <summary>
        /// Gets the row letter of the well (e.g., 'A', 'B', 'C').
        /// </summary>
        public char Row { get; }
        /// <summary>
        /// Gets the column number of the well.
        /// </summary>
        public int Column { get; }

        private string name;
        /// <summary>
        /// Gets or sets the well name (e.g., "A1", "H12").
        /// </summary>
        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        private bool isSelected;
        /// <summary>
        /// Gets or sets whether this well is currently selected.
        /// </summary>
        public bool IsSelected
        {
            get => isSelected;
            set => SetProperty(ref isSelected, value);
        }

        private bool isVisible;
        /// <summary>
        /// Gets or sets whether this well is visible in the UI.
        /// </summary>
        public bool IsVisible
        {
            get => isVisible;
            set => SetProperty(ref isVisible, value);
        }

        private CalibrationPoint calibration;
        /// <summary>
        /// Gets or sets the calibration point associated with this well.
        /// </summary>
        public CalibrationPoint Calibration
        {
            get => calibration;
            set
            {
                SetProperty(ref calibration, value);

                OnPropertyChanged(nameof(IsCalibrated));
            }
        }

        /// <summary>
        /// Gets whether this well has been calibrated.
        /// </summary>
        public bool IsCalibrated => Calibration != null;
        /// <summary>
        /// Initializes a new instance of the WellItem class.
        /// </summary>
        /// <param name="row">The row letter.</param>
        /// <param name="column">The column number.</param>
        /// <param name="isVisible">Whether the well is visible (default: true).</param>
        public WellItem(char row, int column, bool isVisible = true)
        {
            Row = row;
            Column = column;
            Name = $"{row}{column}";
            IsVisible = isVisible;
        }
    }
}
