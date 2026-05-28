using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PHIL_GUI.Models
{
    public enum PlateType
    {
        OrganOnChip,
        Well96
    };
    public class AppSettings : ObservableObject, IPlateContext
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

        public AppKeyBindings AppKeyBindings { get; } = new AppKeyBindings();

        public AppSettings()
        { }
    }
}
