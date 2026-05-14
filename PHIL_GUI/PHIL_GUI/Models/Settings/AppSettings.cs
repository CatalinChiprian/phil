using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PHIL_GUI.Models
{
    public class AppSettings : ObservableObject
    {
        private PlateType selectedPlateType;
        public PlateType SelectedPlateType
        {
            get => selectedPlateType;
            set
            {
                SetProperty(ref selectedPlateType, value);
                OnPropertyChanged(nameof(Is96Well));
            }
        }

        public bool Is96Well => SelectedPlateType == PlateType.Well96;

        private AppKeyBindings appKeyBindings;
        public AppKeyBindings AppKeyBindings
        {
            get => appKeyBindings;
            set => SetProperty(ref appKeyBindings, value);
        }

        public AppSettings()
        {
            SelectedPlateType = PlateType.OrganOnChip;
            AppKeyBindings = new AppKeyBindings();
        }
    }
}
