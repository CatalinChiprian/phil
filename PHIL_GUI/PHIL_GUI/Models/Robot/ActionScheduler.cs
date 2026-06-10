using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PHIL_GUI.Models
{
    /// <summary>
    /// Manages the scheduling and organization of robotic actions.
    /// Maintains action collections and associates actions with specific wells.
    /// </summary>
    public class ActionScheduler
    {
        const int MAX_ALLOWED_SECONDS_DRIFT = 5;

        private int nextTempId = -1;

        /// <summary>
        /// Gets or sets the maximum total number of actions allowed in the system.
        /// </summary>
        public int MaxTotalActions { get; set; }
        /// <summary>
        /// Gets or sets the maximum number of actions allowed per well.
        /// </summary>
        public int MaxActionsPerWell { get; set; }
        /// <summary>
        /// Gets the collection of all scheduled actions.
        /// </summary>
        public ObservableCollection<ScheduleAction> Actions { get; } = new ObservableCollection<ScheduleAction>();
        /// <summary>
        /// Gets the dictionary mapping well indices to their associated actions.
        /// </summary>
        public Dictionary<int, ObservableCollection<ScheduleAction>> WellActions { get; } = new Dictionary<int, ObservableCollection<ScheduleAction>>();
        public ActionScheduler() { }

        /// <summary>
        /// Creates a new scheduled action from an ActionItem and adds it to the Actions collection.
        /// Sets the Model reference on the ActionItem.
        /// </summary>
        /// <param name="action">The ActionItem to create a schedule for.</param>
        public void CreateAction(ActionItem action)
        {
            if (action == null) return;
            ScheduleAction scheduleAction = new ScheduleAction(action);
            if (Actions.Contains(scheduleAction)) return;
            if (Actions.Any(a => a.Id == action.Id)) return;

            action.Model = scheduleAction;
            Actions.Add(scheduleAction);
        }
        /// <summary>
        /// Updates an existing action's properties from an ActionItem.
        /// </summary>
        /// <param name="action">The ActionItem with updated values.</param>
        public void UpdateAction(ActionItem action)
        {
            if (action == null) return;
            ScheduleAction model = action.Model;
            if (model == null) return;

            model.UpdateFromActionItem(action);

        }
        /// <summary>
        /// Updates an action's Id from a temporary Id to a permanent Id.
        /// </summary>
        /// <param name="tempId">The temporary Id.</param>
        /// <param name="id">The new permanent Id.</param>
        public void UpdateAction(int tempId, int id)
        {
            ScheduleAction action = Actions.FirstOrDefault(a => a.Id == tempId);
            if (action == null) return;

            action.Id = id;
        }

        /// <summary>
        /// Updates the last run time for a specific action.
        /// </summary>
        /// <param name="actionId">The action Id.</param>
        /// <param name="lastRunEpoch">The last run time in Unix epoch seconds.</param>
        public void UpdateAction(int actionId, long lastRunEpoch)
        {
            ScheduleAction action = Actions.FirstOrDefault(a => a.Id == actionId);
            if (action == null) return;
            action.LastRunEpoch = lastRunEpoch;
        }
        /// <summary>
        /// Deletes an action from the Actions collection.
        /// </summary>
        /// <param name="actionId">The Id of the action to delete.</param>
        public void DeleteAction(int actionId)
        { 
            ScheduleAction action = Actions.FirstOrDefault(a => a.Id == actionId);
            if (action == null) return;

            Actions.Remove(action);
        }

        /// <summary>
        /// Adds multiple actions to a specific well by their action Ids.
        /// </summary>
        /// <param name="actionIds">Set of action Ids to add.</param>
        /// <param name="wellIndex">The well index to add the actions to.</param>
        public void AddWellActions(HashSet<int> actionIds, int wellIndex)
        {
            List<ScheduleAction> actions = Actions
                .Where(a => actionIds.Contains(a.Id))
                .ToList();

            if (actions.Count == 0) return;

            if (!WellActions.ContainsKey(wellIndex))
            {
                WellActions[wellIndex] = new ObservableCollection<ScheduleAction>();
            }

            foreach (ScheduleAction action in actions)
            {
                if (WellActions[wellIndex].Contains(action)) continue;

                WellActions[wellIndex].Add(action);
            }
        }

        /// <summary>
        /// Attaches an action to multiple selected wells.
        /// </summary>
        /// <param name="action">The action to attach.</param>
        /// <param name="selectedWellIndices">Collection of well indices to attach the action to.</param>
        public void AttachAction(ScheduleAction action, IEnumerable<int> selectedWellIndices)
        {
            foreach (int selectedIndex in selectedWellIndices)
            {
                if (!WellActions.ContainsKey(selectedIndex))
                {
                    WellActions[selectedIndex] = new ObservableCollection<ScheduleAction>();
                }

                if (WellActions[selectedIndex].Contains(action)) continue;

                WellActions[selectedIndex].Add(action);
            }
        }

        /// <summary>
        /// Detaches an action from multiple selected wells.
        /// </summary>
        /// <param name="action">The action to detach.</param>
        /// <param name="selectedWellIndices">Collection of well indices to detach the action from.</param>
        public void DetachAction(ScheduleAction action, IEnumerable<int> selectedWellIndices)
        {
            foreach (int selectedIndex in selectedWellIndices)
            {
                if (!WellActions.ContainsKey(selectedIndex)) continue;

                if (!WellActions[selectedIndex].Contains(action)) continue;

                WellActions[selectedIndex].Remove(action);
            }
        }

        /// <summary>
        /// Validates whether the robot's time is synchronized with the system time.
        /// </summary>
        /// <param name="robotUnixTime">The robot's current time in Unix epoch seconds.</param>
        /// <returns>True if the time difference is less than the allowed drift; otherwise, false.</returns>
        public bool IsRobotTimeValid(long robotUnixTime)
        {
            DateTimeOffset robotTime = DateTimeOffset.FromUnixTimeSeconds(robotUnixTime);

            DateTimeOffset now = DateTimeOffset.UtcNow;

            double diffSeconds = Math.Abs((now - robotTime).TotalSeconds);

            return diffSeconds < MAX_ALLOWED_SECONDS_DRIFT;
        }

        /// <summary>
        /// Gets the next temporary Id for a new action before it's persisted.
        /// Temporary Ids are negative and decrement.
        /// </summary>
        /// <returns>The next available temporary Id.</returns>
        public int GetNextTempId()
        {
            return nextTempId--;
        }

    }
}
