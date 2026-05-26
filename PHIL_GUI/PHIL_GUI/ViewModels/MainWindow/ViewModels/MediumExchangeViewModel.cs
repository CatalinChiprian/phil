using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using PHIL_GUI.Helpers;
using PHIL_GUI.Models;
using PHIL_GUI.ViewModels.Base;
using System;
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
        public bool IsDetailPageVisible => AppSettings.Is96Well ? 
            WellPlateItem96.SelectedWellItems.Count > 0 : 
            WellPlateItemOoC.SelectedWellPairs.Count > 0;

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
                ? wellPairs[0].PairIndex.ToString()
                : $"{wellPairs.Count} channels";

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
                    
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (ScheduleAction delAction in e.OldItems)
                {
                    ActionItem item = ActionItems.FirstOrDefault(a => a.Id == delAction.Id);

                    if (item == null) continue;

                    ActionItems.Remove(item);
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

            LoadCurrentWellActions(target);
            LoadAvailableActions();

            OnPropertyChanged(nameof(SelectedWellText));
            OnPropertyChanged(nameof(IsDetailPageVisible));
            OnPropertyChanged(nameof(SelectedWellsCount));
        }

        private void LoadCurrentWellActions(string target)
        {
            int wellIndex = target.ToIndex();

            if (!ActionScheduler.WellActions.TryGetValue(wellIndex, out List<ScheduleAction> actions) || actions.Count == 0)
            {
                CurrentWellActions.Clear();
                return;
            }

            // FIRST selection
            if (CurrentWellActions.Count == 0)
            {
                foreach (ScheduleAction action in actions)
                    CurrentWellActions.Add(new ActionItem(action));

                return;
            }

            // INTERSECTION
            HashSet<int> matchingIds = actions.Select(a => a.Id).ToHashSet();
            List<ActionItem> filteredActions = CurrentWellActions.Where(cw => matchingIds.Contains(cw.Id)).ToList();

            CurrentWellActions.Clear();

            foreach (ActionItem action in filteredActions)
                CurrentWellActions.Add(action);
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
