using CommunityToolkit.Mvvm.Input;
using PHIL_GUI.Models;
using PHIL_GUI.ViewModels.Base;
using System.Windows.Input;

namespace PHIL_GUI.ViewModels
{
    public class MediumExchangeViewModel : ViewModelBase
    {
        public ICommand SelectTargetCommand { get; }
        public ICommand SelectAllCommand { get; }
        public ICommand ClearSelectionCommand { get; }
        public ICommand SelectQ1Command { get; }
        public ICommand SelectQ2Command { get; }
        public ICommand SelectQ3Command { get; }
        public ICommand SelectQ4Command { get; }

        public IWellPlateItem WellPlate { get; private set; }
        public WellPlateItemOoC? WellPlateItemOoC => WellPlate as WellPlateItemOoC;
        public WellPlateItem96? WellPlateItem96 => WellPlate as WellPlateItem96;
        public bool IsDetailPageVisible => AppSettings.Is96Well ? 
            WellPlateItem96.SelectedWellItems.Count > 0 : 
            WellPlateItemOoC.SelectedWellPairs.Count > 0;

        public string SelectedWellsCount => AppSettings.Is96Well
            ? GetWellText(WellPlateItem96.SelectedWellItems.Count)
            : GetPairText(WellPlateItemOoC.SelectedWellPairs.Count);

        private string GetWellText(int count) => $"Selected {count} well{(count == 1 ? "" : "s")}";

        private string GetPairText(int count) => $"Selected {count} pair{(count == 1 ? "" : "s")}";

        public string ActionCount => GetActionText(ActionScheduler.WellActions.Count);

        private string GetActionText(int count) => $"Scheduled {count} action{(count == 1 ? "" : "s")}";

        public Well CurrentWell => RobotProtocolService.RobotState.CurrentWell;
        public AppSettings AppSettings => AppSettingsService.AppSettings;
        public ActionScheduler ActionScheduler => RobotProtocolService.RobotState.ActionScheduler;
        public Calibration Calibration => RobotProtocolService.RobotState.Calibration;

        public MediumExchangeViewModel() 
        {
            WellPlate = AppSettings.Is96Well ? new WellPlateItem96() : new WellPlateItemOoC();
            WellPlate.AllowMultipleSelection = true;

            SelectTargetCommand = new RelayCommand<string>(SelectTarget);
            SelectAllCommand = new RelayCommand(SelectAllTargets);
            ClearSelectionCommand = new RelayCommand(ClearSelection);
            SelectQ1Command = new RelayCommand(() => SelectQuad(1));
            SelectQ2Command = new RelayCommand(() => SelectQuad(2));
            SelectQ3Command = new RelayCommand(() => SelectQuad(3));
            SelectQ4Command = new RelayCommand(() => SelectQuad(4));
            AppSettings.PropertyChanged += AppSettings_PropertyChanged;
        }

        private void SelectTarget(string target)
        {
            if (AppSettings.Is96Well)
            {
                WellPlateItem96.SelectWell(target);
            }
            else
            {
                WellPlateItemOoC.SelectWellPair(int.Parse(target));
            }

            OnPropertyChanged(nameof(IsDetailPageVisible));
            OnPropertyChanged(nameof(SelectedWellsCount));
        }

        private void SelectAllTargets()
        {
            if (AppSettings.Is96Well)
            {
                WellPlateItem96.SelectAllWells();
            }
            else
            {
                WellPlateItemOoC.SelectAllPairs();
            }

            OnPropertyChanged(nameof(IsDetailPageVisible));
            OnPropertyChanged(nameof(SelectedWellsCount));
        }

        private void SelectQuad(int quadNumber)
        {
            if (AppSettings.Is96Well)
            {
            }
            else
            {
                WellPlateItemOoC.SelectQuadrantPairs(quadNumber);
            }

            OnPropertyChanged(nameof(IsDetailPageVisible));
            OnPropertyChanged(nameof(SelectedWellsCount));
        }

        private void ClearSelection()
        {
            if (AppSettings.Is96Well)
            {
                WellPlateItem96.ClearSelection();
            }
            else
            {
                WellPlateItemOoC.ClearPairSelection();
            }

            OnPropertyChanged(nameof(IsDetailPageVisible));
            OnPropertyChanged(nameof(SelectedWellsCount));
        }

        private void AppSettings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AppSettings.SelectedPlateType))
                OverrideWellPlate();
        }

        private void OverrideWellPlate()
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
