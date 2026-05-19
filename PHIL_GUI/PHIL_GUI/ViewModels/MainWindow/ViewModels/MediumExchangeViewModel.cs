using CommunityToolkit.Mvvm.Input;
using PHIL_GUI.Models;
using PHIL_GUI.ViewModels.Base;
using System.Windows.Input;

namespace PHIL_GUI.ViewModels
{
    public class MediumExchangeViewModel : ViewModelBase
    {
        public ICommand SelectTargetCommand { get; }

        public IWellPlateItem WellPlate { get; private set; }
        public WellPlateItemOoC? WellPlateItemOoC => WellPlate as WellPlateItemOoC;
        public WellPlateItem96? WellPlateItem96 => WellPlate as WellPlateItem96;
        public bool IsDetailPageVisible => AppSettings.Is96Well ? 
            WellPlateItem96.SelectedWellItems.Count > 0 : 
            WellPlateItemOoC.SelectedWellPairs.Count > 0;

        public Well CurrentWell => RobotProtocolService.RobotState.CurrentWell;
        public AppSettings AppSettings => AppSettingsService.AppSettings;
        public Calibration Calibration => RobotProtocolService.RobotState.Calibration;

        public MediumExchangeViewModel() 
        {
            WellPlate = AppSettings.Is96Well ? new WellPlateItem96() : new WellPlateItemOoC();
            WellPlate.AllowMultipleSelection = true;

            SelectTargetCommand = new RelayCommand<string>(SelectTarget);

            AppSettings.PropertyChanged += AppSettings_PropertyChanged;
        }

        private void SelectTarget(string target)
        {
            if (AppSettings.Is96Well)
            {
                WellPlate.SelectWell(target);
            }
            else
            {
                WellPlateItemOoC.SelectWellPair(int.Parse(target));
            }

            OnPropertyChanged(nameof(IsDetailPageVisible));
        }

        private void AppSettings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AppSettings.SelectedPlateType))
                OverrideWellPlate();
        }

        void OverrideWellPlate()
        {
            string selectedWellName = CurrentWell.Name;

            if (AppSettings.Is96Well)
            {
                WellPlate = new WellPlateItem96();

                OnPropertyChanged(nameof(WellPlateItem96));
            }
            else
            {
                WellPlate = new WellPlateItemOoC();

                OnPropertyChanged(nameof(WellPlateItemOoC));
            }

            WellPlate.AllowMultipleSelection = true;
            WellPlate.SelectWell(selectedWellName);
        }
    }
}
