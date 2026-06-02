using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PHIL_GUI.Models
{
    public class ActionScheduler
    {
        const int MAX_ALLOWED_SECONDS_DRIFT = 5;

        private int nextTempId = -1;

        public int MaxTotalActions { get; set; }
        public int MaxActionsPerWell { get; set; }
        public ObservableCollection<ScheduleAction> Actions { get; } = new ObservableCollection<ScheduleAction>();
        public Dictionary<int, ObservableCollection<ScheduleAction>> WellActions { get; } = new Dictionary<int, ObservableCollection<ScheduleAction>>();
        public ActionScheduler() { }

        public void CreateAction(ActionItem action)
        {
            if (action == null) return;
            ScheduleAction scheduleAction = new ScheduleAction(action);
            if (Actions.Contains(scheduleAction)) return;
            if (Actions.Any(a => a.Id == action.Id)) return;

            action.Model = scheduleAction;
            Actions.Add(scheduleAction);
        }
        public void UpdateAction(ActionItem action)
        {
            if (action == null) return;
            ScheduleAction model = action.Model;
            if (model == null) return;

            model.UpdateFromActionItem(action);

        }
        public void UpdateAction(int tempId, int id)
        {
            ScheduleAction action = Actions.FirstOrDefault(a => a.Id == tempId);
            if (action == null) return;

            action.Id = id;
        }

        public void UpdateAction(int actionId, long lastRunEpoch)
        {
            ScheduleAction action = Actions.FirstOrDefault(a => a.Id == actionId);
            if (action == null) return;
            action.LastRunEpoch = lastRunEpoch;
        }
        public void DeleteAction(int actionId)
        { 
            ScheduleAction action = Actions.FirstOrDefault(a => a.Id == actionId);
            if (action == null) return;

            Actions.Remove(action);
        }

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

        public void DetachAction(ScheduleAction action, IEnumerable<int> selectedWellIndices)
        {
            foreach (int selectedIndex in selectedWellIndices)
            {
                if (!WellActions.ContainsKey(selectedIndex)) continue;

                if (!WellActions[selectedIndex].Contains(action)) continue;

                WellActions[selectedIndex].Remove(action);
            }
        }

        public bool IsRobotTimeValid(long robotUnixTime)
        {
            DateTimeOffset robotTime = DateTimeOffset.FromUnixTimeSeconds(robotUnixTime);

            DateTimeOffset now = DateTimeOffset.UtcNow;

            double diffSeconds = Math.Abs((now - robotTime).TotalSeconds);

            return diffSeconds < MAX_ALLOWED_SECONDS_DRIFT;
        }

        public int GetNextTempId()
        {
            return nextTempId--;
        }

    }
}
