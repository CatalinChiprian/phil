using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PHIL_GUI.Models
{
    public class ActionScheduler
    {
        private int nextTempId = -1;
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
            int index = Actions.IndexOf(action);
            action.Id = id;
        }
        public void DeleteAction(int actionId)
        { 
            ScheduleAction action = Actions.FirstOrDefault(a => a.Id == actionId);
            if (action == null) return;

            Actions.Remove(action);
        }
        
        public void AttachAction(ActionItem action, IEnumerable<int> selectedWellIndices)
        {
            foreach (int selectedIndex in selectedWellIndices)
            {
                if (!WellActions.ContainsKey(selectedIndex))
                {
                    WellActions[selectedIndex] = new ObservableCollection<ScheduleAction>();
                }

                WellActions[selectedIndex].Add(action.Model);
            }
        }

        public void DetachAction(ActionItem action, IEnumerable<int> selectedWellIndices)
        {
            foreach (int selectedIndex in selectedWellIndices)
            {
                WellActions[selectedIndex].Remove(action.Model);
            }
        }

        public int GetNextTempId()
        {
            return nextTempId--;
        }

    }
}
