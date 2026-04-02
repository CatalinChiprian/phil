using CommunityToolkit.Mvvm.ComponentModel;

namespace PHIL_GUI.Models
{
    public enum MoveState
    {
        Idle,
        Moving,
        EmergencyStopped,
    }

    public enum PlateType
    {
        Well96,
        OrganOnChip
    };
    public class Settings : ObservableObject
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

        private MoveState state;
        public MoveState State
        {
            get => state;
            set => SetProperty(ref state, value);
        }

        private double stepSize;
        public double StepSize
        {
            get => stepSize;
            set => SetProperty(ref stepSize, value);
        }
    }
}
