using CommunityToolkit.Mvvm.Input;
using PHIL_GUI.Helpers;
using PHIL_GUI.Models;
using PHIL_GUI.ViewModels.Base;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
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

        public ObservableCollection<ActionItem> ActionItems { get; } = new ObservableCollection<ActionItem>();
        public ObservableCollection<ActionItem> CurrentWellActions { get; } = new ObservableCollection<ActionItem>();
        public ObservableCollection<ActionItem> AvailableWellActions { get; } = new ObservableCollection<ActionItem>();

        public IWellPlateItem WellPlate { get; private set; }
        public WellPlateItemOoC? WellPlateItemOoC => WellPlate as WellPlateItemOoC;
        public WellPlateItem96? WellPlateItem96 => WellPlate as WellPlateItem96;

        private string lastSelectedTarget = string.Empty;
        public int DetailsPageWidth => AppSettings.Is96Well ? 
            (WellPlateItem96.SelectedWellItems.Count > 0 ? 300 : 0) : 
            (WellPlateItemOoC.SelectedWellPairs.Count > 0 ? 300 : 0);

        public string SelectedWellsCount => AppSettings.Is96Well
            ? GetWellCountText(WellPlateItem96.SelectedWellItems.Count)
            : GetChannelCountText(WellPlateItemOoC.SelectedWellPairs.Count);

        private string GetWellCountText(int count) => $"Selected {count} well{(count == 1 ? "" : "s")}";

        private string GetChannelCountText(int count) => $"Selected {count} channels{(count == 1 ? "" : "s")}";

        public string ActionCount => GetActionText(ActionScheduler.WellActions.Count);

        private string GetActionText(int count) => $"Scheduled {count} action{(count == 1 ? "" : "s")}";
        public string SelectedWellText => AppSettings.Is96Well
            ? GetWellText(WellPlateItem96.SelectedWellItems)
            : GetChannelText(WellPlateItemOoC.SelectedWellPairs);

        private string GetWellText(List<WellItem> wellItems) =>
            wellItems.Count == 1
                ? wellItems[0].Name
                : $"{wellItems.Count} wells";
        private string GetChannelText(List<WellPairItem> wellPairs) =>
            wellPairs.Count == 1
                ? $"Channel {wellPairs[0].PairIndex.ToString()}"
                : $"{wellPairs.Count} channels";

        public string WellActionsText => AppSettings.Is96Well
            ? GetWellActionsText(WellPlateItem96.SelectedWellItems.Count)
            : GetWellActionsText(WellPlateItemOoC.SelectedWellPairs.Count);

        private string GetWellActionsText(int count) =>
            count == 1
                ? $"{CurrentWellActions.Count} action{(CurrentWellActions.Count == 1 ? "" : "s")} attached"
                : $"{CurrentWellActions.Count} shared action{(CurrentWellActions.Count == 1 ? "" : "s")}";

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
            ActionScheduler.Actions.CollectionChanged += Actions_CollectionChanged;
        }

        private void Actions_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (ScheduleAction newAction in e.NewItems)
                {
                    ActionItem item = new ActionItem(newAction);
                    SetItemVisibility(item);
                    ActionItems.Add(item);

                    RefreshWellActionsList();

                }
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (ScheduleAction delAction in e.OldItems)
                {
                    ActionItem item = ActionItems.FirstOrDefault(a => a.Id == delAction.Id);

                    if (item == null) continue;

                    ActionItems.Remove(item);

                    RefreshWellActionsList();
                }
            }
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

            lastSelectedTarget = target;

            RefreshWellActionsList();

            OnPropertyChanged(nameof(SelectedWellText));
            OnPropertyChanged(nameof(WellActionsText));
            OnPropertyChanged(nameof(DetailsPageWidth));
            OnPropertyChanged(nameof(SelectedWellsCount));
        }

        public void AttachAction(ActionItem action)
        {
            IEnumerable<string> selectedWellNames = GetSelectedWellNames();

            IEnumerable<int> selectedWellIndices = selectedWellNames.ToIndex();

            RobotProtocolService.AttachAction(action.Model, selectedWellIndices);

            RefreshWellActionsList();
        }
        public void DetachAction(ActionItem action)
        {
            IEnumerable<string> selectedWellNames = GetSelectedWellNames();

            IEnumerable<int> selectedWellIndices = selectedWellNames.ToIndex();

            RobotProtocolService.DetachAction(action.Model, selectedWellIndices);

            RefreshWellActionsList();
        }

        private IEnumerable<string> GetSelectedWellNames()
        {
            List<string> selectedWellNames = new List<string>();
            if (AppSettings.Is96Well)
            {
                selectedWellNames = WellPlateItem96.SelectedWellItems.Select(w => w.Name).ToList();
            }
            else
            {
                var selectedPairs = WellPlateItemOoC.SelectedWellPairs;
                foreach (WellPairItem pair in selectedPairs)
                {
                    selectedWellNames.Add(pair.In.Name);
                }
            }

            return selectedWellNames;
        }

        private void RefreshWellActionsList()
        {
            LoadCurrentWellActions(lastSelectedTarget);
            LoadAvailableActions();

            OnPropertyChanged(nameof(WellActionsText));
        }

        private void LoadCurrentWellActions(string target)
        {
            List<int> selectedIndices = GetSelectedWellNames().ToIndex().ToList();

            CurrentWellActions.Clear();

            var allSets = selectedIndices
                .Select(i => ActionScheduler.WellActions.TryGetValue(i, out var list)
                    ? list.Select(a => a.Id).ToHashSet()
                    : new HashSet<int>())
                .ToList();


            if (selectedIndices.Count == 0) return;

            var intersection = allSets.Aggregate((a, b) =>
            {
                a.IntersectWith(b);
                return a;
            });

            foreach (var actionItem in ActionItems)
            {
                if (intersection.Contains(actionItem.Id))
                {
                    CurrentWellActions.Add(actionItem);
                }
            }
        }

        private void LoadAvailableActions()
        {
            AvailableWellActions.Clear();

            HashSet<int> existingIds = CurrentWellActions.Select(cw => cw.Id).ToHashSet();


            foreach (ActionItem action in ActionItems)
            {
                if (existingIds.Contains(action.Id)) continue;

                AvailableWellActions.Add(action);
            }
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

            OnPropertyChanged(nameof(SelectedWellText));
            OnPropertyChanged(nameof(WellActionsText));
            OnPropertyChanged(nameof(DetailsPageWidth));
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

            OnPropertyChanged(nameof(SelectedWellText));
            OnPropertyChanged(nameof(WellActionsText));
            OnPropertyChanged(nameof(DetailsPageWidth));
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

            OnPropertyChanged(nameof(SelectedWellText));
            OnPropertyChanged(nameof(WellActionsText));
            OnPropertyChanged(nameof(DetailsPageWidth));
            OnPropertyChanged(nameof(SelectedWellsCount));
        }
        public void DeleteAction(int actionId)
        {
            RobotProtocolService.DeleteAction(actionId);
            OnPropertyChanged(nameof(ActionCount));
        }

        private void AppSettings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AppSettings.SelectedPlateType))
            {
                OverrideWellPlate();
                OverrideItemsVisibility();
            }
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
        private void OverrideItemsVisibility()
        {
            foreach (ActionItem item in ActionItems)
            {
                SetItemVisibility(item);
            }
        }
        private void SetItemVisibility(ActionItem item)
        {
            item.IsVisible = (item.Type == ActionType.Exchange) != AppSettings.Is96Well;
        }
    }
}
// TO DO SEE SCROLLVIEWER ON ACTIONS

