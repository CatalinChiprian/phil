using CommunityToolkit.Mvvm.Input;
using PHIL_GUI.Models;
using PHIL_GUI.ViewModels.Base;
using System.Windows.Input;

namespace PHIL_GUI.ViewModels
{
    /// <summary>
    /// ViewModel for the Plate settings page, managing well plate type configuration.
    /// Allows users to switch between 96-well plates and organ-on-chip plates.
    /// </summary>
    public class PlateViewModel : ViewModelBase, ISettingsPage
    {
        /// <summary>
        /// Gets the command to select the 96-well plate type.
        /// </summary>
        public ICommand SelectWell96Command { get; }

        /// <summary>
        /// Gets the command to select the organ-on-chip plate type.
        /// </summary>
        public ICommand SelectOrganOnChipCommand { get; }

        private PlateType plateType;
        /// <summary>
        /// Gets or sets the currently selected plate type.
        /// </summary>
        public PlateType PlateType
        {
            get => plateType;
            set
            {
                SetProperty(ref plateType, value);

                OnPropertyChanged(nameof(Is96Well));
            }
        }

        /// <summary>
        /// Gets a value indicating whether the 96-well plate type is currently selected.
        /// </summary>
        public bool Is96Well => PlateType == PlateType.Well96;

        /// <summary>
        /// Initializes a new instance of the PlateViewModel class.
        /// Loads the current plate type setting and sets up selection commands.
        /// </summary>
        public PlateViewModel()
        {
            PlateType = AppSettingsService.AppSettings.SelectedPlateType;
            SelectWell96Command = new RelayCommand(() => PlateType = PlateType.Well96);
            SelectOrganOnChipCommand = new RelayCommand(() => PlateType = PlateType.OrganOnChip);
        }

        /// <summary>
        /// Applies the selected plate type to the application settings and notifies the robot.
        /// </summary>
        public void ApplyChanges()
        {
            if (AppSettingsService.AppSettings.SelectedPlateType != PlateType)
            {
                AppSettingsService.AppSettings.SelectedPlateType = PlateType;
                RobotProtocolService.SetWellPlateType(PlateType);
            }
        }
    }
}
