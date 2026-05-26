using CommunityToolkit.Mvvm.Input;
using PHIL_GUI.Models;
using PHIL_GUI.ViewModels.Base;
using System.Windows.Input;

namespace PHIL_GUI.ViewModels
{
    public class PlateViewModel : ViewModelBase, ISettingsPage
    {
        public ICommand SelectWell96Command { get; }
        public ICommand SelectOrganOnChipCommand { get; }

        private PlateType plateType;
        public PlateType PlateType
        {
            get => plateType;
            set
            {
                SetProperty(ref plateType, value);

                OnPropertyChanged(nameof(Is96Well));
            }
        }
        public bool Is96Well => PlateType == PlateType.Well96;
        public PlateViewModel()
        {
            PlateType = AppSettingsService.AppSettings.SelectedPlateType;
            SelectWell96Command = new RelayCommand(() => PlateType = PlateType.Well96);
            SelectOrganOnChipCommand = new RelayCommand(() => PlateType = PlateType.OrganOnChip);
        }

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
