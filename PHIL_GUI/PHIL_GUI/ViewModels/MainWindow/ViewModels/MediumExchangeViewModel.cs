using Avalonia.Threading;
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
        private const int DETAILS_PAGE_WIDTH = 300;
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
        public int DetailsPageWidth => WellPlate.SelectedCount > 0 ? DETAILS_PAGE_WIDTH : 0;

        private readonly DispatcherTimer timer;

        public string SelectedWellsCount => AppSettings.Is96Well
            ? GetWellCountText(WellPlate.SelectedCount)
            : GetChannelCountText(WellPlate.SelectedCount);

        private string GetWellCountText(int count) => $"Selected {count} well{(count == 1 ? "" : "s")}";

        private string GetChannelCountText(int count) => $"Selected {count} channels{(count == 1 ? "" : "s")}";

        public string ActionCount => GetActionText(ActionScheduler.WellActions.Count);

        private string GetActionText(int count) => $"Scheduled {count} action{(count == 1 ? "" : "s")}";
        public string SelectedWellText => GetTargetText(WellPlate.GetSelectedNames());
        private string GetTargetText(List<string> targets)
        {
            if (targets.Count == 0)
                return string.Empty;

            if (targets.Count == 1)
            {
                return AppSettings.Is96Well
                    ? targets[0]
                    : $"Channel {targets[0]}";
            }

            return $"{targets.Count} {GetTargetTypeName()}s";
        }
        private string GetTargetTypeName() => AppSettings.Is96Well ? "Well" : "Channel";

        public string WellActionsText => GetWellActionsText(WellPlate.SelectedCount);

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

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            timer.Tick += (_, _) => UpdateCountdowns();
            timer.Start();


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

        private void UpdateCountdowns()
        {
            foreach (ActionItem item in ActionItems)
            {
                item.UpdateCountdown();
            }
        }

        private void SelectTarget(string target)
        {
            WellPlate.Select(target);
            lastSelectedTarget = target;
            RefreshWellActionsList();

            RefreshUI();
        }

        public void AttachAction(ActionItem action)
        {
            IEnumerable<string> selectedWellNames = WellPlate.GetSelectedWellNames();

            IEnumerable<int> selectedWellIndices = selectedWellNames.ToIndices();

            RobotProtocolService.AttachAction(action.Model, selectedWellIndices);

            RefreshWellActionsList();
        }
        public void DetachAction(ActionItem action)
        {
            IEnumerable<string> selectedWellNames = WellPlate.GetSelectedWellNames();

            IEnumerable<int> selectedWellIndices = selectedWellNames.ToIndices();

            RobotProtocolService.DetachAction(action.Model, selectedWellIndices);

            RefreshWellActionsList();
        }

        private void RefreshWellActionsList()
        {
            LoadCurrentWellActions(lastSelectedTarget);
            LoadAvailableActions();

            OnPropertyChanged(nameof(WellActionsText));
        }

        private void LoadCurrentWellActions(string target)
        {
            IEnumerable<string> selectedWellNames = WellPlate.GetSelectedWellNames();

            List<int> selectedIndices = selectedWellNames.ToIndices().ToList();

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
            WellPlate.SelectAll();

            RefreshUI();
        }

        private void SelectQuad(int quadNumber)
        {
            WellPlate.SelectQuadrant(quadNumber);

            RefreshUI();
        }

        private void ClearSelection()
        {
            WellPlate.Clear();

            RefreshUI();
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

        private void RefreshUI()
        {
            OnPropertyChanged(nameof(SelectedWellText));
            OnPropertyChanged(nameof(WellActionsText));
            OnPropertyChanged(nameof(DetailsPageWidth));
            OnPropertyChanged(nameof(SelectedWellsCount));
        }
    }
}

