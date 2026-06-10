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
    /// <summary>
    /// ViewModel for the Medium Exchange page, managing scheduled actions and well/channel selection.
    /// Allows users to create, attach, and schedule automated medium exchange and dispense actions.
    /// </summary>
    public class MediumExchangeViewModel : ViewModelBase
    {
        private const int DETAILS_PAGE_WIDTH = 300;

        /// <summary>
        /// Gets the command to select a specific well or channel target.
        /// </summary>
        public ICommand SelectTargetCommand { get; }

        /// <summary>
        /// Gets the command to select all wells or channels.
        /// </summary>
        public ICommand SelectAllCommand { get; }

        /// <summary>
        /// Gets the command to clear the current well/channel selection.
        /// </summary>
        public ICommand ClearSelectionCommand { get; }

        /// <summary>
        /// Gets the command to select all wells/channels in quadrant 1.
        /// </summary>
        public ICommand SelectQ1Command { get; }

        /// <summary>
        /// Gets the command to select all wells/channels in quadrant 2.
        /// </summary>
        public ICommand SelectQ2Command { get; }

        /// <summary>
        /// Gets the command to select all wells/channels in quadrant 3.
        /// </summary>
        public ICommand SelectQ3Command { get; }

        /// <summary>
        /// Gets the command to select all wells/channels in quadrant 4.
        /// </summary>
        public ICommand SelectQ4Command { get; }

        /// <summary>
        /// Gets the collection of all action items for display and management.
        /// </summary>
        public ObservableCollection<ActionItem> ActionItems { get; } = new ObservableCollection<ActionItem>();

        /// <summary>
        /// Gets the collection of actions currently attached to the selected wells/channels.
        /// </summary>
        public ObservableCollection<ActionItem> CurrentWellActions { get; } = new ObservableCollection<ActionItem>();

        /// <summary>
        /// Gets the collection of actions available to attach to the selected wells/channels.
        /// </summary>
        public ObservableCollection<ActionItem> AvailableWellActions { get; } = new ObservableCollection<ActionItem>();

        /// <summary>
        /// Gets the current well plate model (either 96-well or organ-on-chip).
        /// </summary>
        public IWellPlateItem WellPlate { get; private set; }

        /// <summary>
        /// Gets the well plate as an organ-on-chip plate, or null if it's a 96-well plate.
        /// </summary>
        public WellPlateItemOoC? WellPlateItemOoC => WellPlate as WellPlateItemOoC;

        /// <summary>
        /// Gets the well plate as a 96-well plate, or null if it's an organ-on-chip plate.
        /// </summary>
        public WellPlateItem96? WellPlateItem96 => WellPlate as WellPlateItem96;

        /// <summary>
        /// Gets the width of the details panel. Returns 300 if wells are selected, otherwise 0.
        /// </summary>
        public int DetailsPageWidth => WellPlate.SelectedCount > 0 ? DETAILS_PAGE_WIDTH : 0;

        private readonly DispatcherTimer timer;

        /// <summary>
        /// Gets the text describing the number of selected wells or channels.
        /// </summary>
        public string SelectedWellsCount => AppSettings.Is96Well
            ? GetWellCountText(WellPlate.SelectedCount)
            : GetChannelCountText(WellPlate.SelectedCount);

        private string GetWellCountText(int count) => $"Selected {count} well{(count == 1 ? "" : "s")}";

        private string GetChannelCountText(int count) => $"Selected {count} channels{(count == 1 ? "" : "s")}";

        /// <summary>
        /// Gets the text describing the total number of scheduled actions.
        /// </summary>
        public string ActionCount => GetActionText(ActionScheduler.WellActions.Count);

        private string GetActionText(int count) => $"Scheduled {count} action{(count == 1 ? "" : "s")}";

        /// <summary>
        /// Gets the text describing the currently selected wells or channels.
        /// </summary>
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

        /// <summary>
        /// Gets the text describing the number of actions attached to the selected wells/channels.
        /// </summary>
        public string WellActionsText => GetWellActionsText(WellPlate.SelectedCount);

        private string GetWellActionsText(int count) =>
            count == 1
                ? $"{CurrentWellActions.Count} action{(CurrentWellActions.Count == 1 ? "" : "s")} attached"
                : $"{CurrentWellActions.Count} shared action{(CurrentWellActions.Count == 1 ? "" : "s")}";

        /// <summary>
        /// Gets a value indicating whether new actions can be created.
        /// Returns false if the maximum number of total actions has been reached.
        /// </summary>
        public bool IsCreateActionEnabled => ActionItems.Count < ActionScheduler.MaxTotalActions;

        /// <summary>
        /// Gets the tooltip text for the create action button, explaining why it may be disabled.
        /// </summary>
        public string CreateActionTooltip => IsCreateActionEnabled
            ? "Create a new action and attach it to the selected wells/channels."
            : $"Maximum of {ActionScheduler.MaxTotalActions} total actions reached. Please delete an existing action before creating a new one.";

        /// <summary>
        /// Gets a value indicating whether an action can be attached to the currently selected wells/channels.
        /// Returns false if any selected well has reached the maximum number of actions per well.
        /// </summary>
        public bool CanAttachAction
        {
            get
            {
                if (!WellPlate.GetSelectedWellNames().Any())
                    return false;

                var selectedIndices = WellPlate.GetSelectedWellNames().ToIndices();

                return selectedIndices.All(i =>
                {
                    if (!ActionScheduler.WellActions.TryGetValue(i, out var list))
                        return true;

                    return list.Count < ActionScheduler.MaxActionsPerWell;
                });
            }
        }

        /// <summary>
        /// Gets the tooltip text for the attach action button, explaining why it may be disabled.
        /// </summary>
        public string AttachActionTooltip => CanAttachAction
            ? "Attach the selected action to the selected wells/channels."
            : $"Cannot attach action. One or more selected wells/channels have reached the maximum of {ActionScheduler.MaxActionsPerWell} actions.";

        /// <summary>
        /// Gets the robot's current well position information.
        /// </summary>
        public Well CurrentWell => RobotProtocolService.RobotState.CurrentWell;

        /// <summary>
        /// Gets the application settings including plate type selection.
        /// </summary>
        public AppSettings AppSettings => AppSettingsService.AppSettings;

        /// <summary>
        /// Gets the action scheduler managing all scheduled actions and well-action mappings.
        /// </summary>
        public ActionScheduler ActionScheduler => RobotProtocolService.RobotState.ActionScheduler;

        /// <summary>
        /// Gets the robot's calibration data.
        /// </summary>
        public Calibration Calibration => RobotProtocolService.RobotState.Calibration;

        /// <summary>
        /// Initializes a new instance of the MediumExchangeViewModel class.
        /// Sets up the well plate, commands, timer for countdown updates, and event subscriptions.
        /// </summary>
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

        /// <summary>
        /// Handles collection changes in the action scheduler's actions list.
        /// Adds or removes action items from the UI collections.
        /// </summary>
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

        /// <summary>
        /// Updates the countdown timers for all action items.
        /// Called periodically by the dispatcher timer.
        /// </summary>
        private void UpdateCountdowns()
        {
            foreach (ActionItem item in ActionItems)
            {
                item.UpdateCountdown();
            }
        }

        /// <summary>
        /// Selects a specific well or channel target and updates the action lists.
        /// </summary>
        /// <param name="target">The well or channel name to select.</param>
        private void SelectTarget(string target)
        {
            WellPlate.Select(target);
            RefreshWellActionsList();

            RefreshUI();
        }

        /// <summary>
        /// Attaches an action to the currently selected wells or channels.
        /// </summary>
        /// <param name="action">The action item to attach.</param>
        public void AttachAction(ActionItem action)
        {
            IEnumerable<string> selectedWellNames = WellPlate.GetSelectedWellNames();

            IEnumerable<int> selectedWellIndices = selectedWellNames.ToIndices();

            RobotProtocolService.AttachAction(action.Model, selectedWellIndices);

            RefreshWellActionsList();
        }

        /// <summary>
        /// Detaches an action from the currently selected wells or channels.
        /// </summary>
        /// <param name="action">The action item to detach.</param>
        public void DetachAction(ActionItem action)
        {
            IEnumerable<string> selectedWellNames = WellPlate.GetSelectedWellNames();

            IEnumerable<int> selectedWellIndices = selectedWellNames.ToIndices();

            RobotProtocolService.DetachAction(action.Model, selectedWellIndices);

            RefreshWellActionsList();
        }

        /// <summary>
        /// Refreshes the current and available well actions lists based on the current selection.
        /// </summary>
        private void RefreshWellActionsList()
        {
            LoadCurrentWellActions();
            LoadAvailableActions();

            OnPropertyChanged(nameof(WellActionsText));
        }

        /// <summary>
        /// Loads the actions that are attached to all currently selected wells/channels.
        /// Only includes actions that are common to all selected wells/channels.
        /// </summary>
        private void LoadCurrentWellActions()
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

        /// <summary>
        /// Loads the actions that are available to attach to the currently selected wells/channels.
        /// Excludes actions that are already attached to all selected wells/channels.
        /// </summary>
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

        /// <summary>
        /// Selects all wells or channels on the current plate.
        /// </summary>
        private void SelectAllTargets()
        {
            WellPlate.SelectAll();

            RefreshUI();
        }

        /// <summary>
        /// Selects all wells or channels in the specified quadrant.
        /// </summary>
        /// <param name="quadNumber">The quadrant number (1-4).</param>
        private void SelectQuad(int quadNumber)
        {
            WellPlate.SelectQuadrant(quadNumber);

            RefreshUI();
        }

        /// <summary>
        /// Clears the current well or channel selection.
        /// </summary>
        private void ClearSelection()
        {
            WellPlate.Clear();

            RefreshUI();
        }

        /// <summary>
        /// Deletes an action by its ID from the robot and the action list.
        /// </summary>
        /// <param name="actionId">The ID of the action to delete.</param>
        public void DeleteAction(int actionId)
        {
            RobotProtocolService.DeleteAction(actionId);
            OnPropertyChanged(nameof(ActionCount));
        }

        /// <summary>
        /// Handles property changes in the application settings.
        /// Updates the well plate and action visibility when the plate type changes.
        /// </summary>
        private void AppSettings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AppSettings.SelectedPlateType))
            {
                OverrideWellPlate();
                OverrideItemsVisibility();
            }
        }

        /// <summary>
        /// Replaces the current well plate model based on the selected plate type in settings.
        /// Preserves the currently selected well after switching plates and enables multiple selection.
        /// </summary>
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

        /// <summary>
        /// Updates the visibility of all action items based on the current plate type.
        /// </summary>
        private void OverrideItemsVisibility()
        {
            foreach (ActionItem item in ActionItems)
            {
                SetItemVisibility(item);
            }
        }

        /// <summary>
        /// Sets the visibility of an action item based on its type and the current plate type.
        /// Exchange actions are visible for organ-on-chip plates, hidden for 96-well plates.
        /// </summary>
        /// <param name="item">The action item to update.</param>
        private void SetItemVisibility(ActionItem item)
        {
            item.IsVisible = (item.Type == ActionType.Exchange) != AppSettings.Is96Well;
        }

        /// <summary>
        /// Refreshes all UI-bound properties to reflect the current state.
        /// </summary>
        private void RefreshUI()
        {
            OnPropertyChanged(nameof(SelectedWellText));
            OnPropertyChanged(nameof(WellActionsText));
            OnPropertyChanged(nameof(DetailsPageWidth));
            OnPropertyChanged(nameof(SelectedWellsCount));
            OnPropertyChanged(nameof(IsCreateActionEnabled));
            OnPropertyChanged(nameof(CreateActionTooltip));
        }
    }
}

