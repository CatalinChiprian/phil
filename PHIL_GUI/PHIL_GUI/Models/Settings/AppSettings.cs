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
    }
}
