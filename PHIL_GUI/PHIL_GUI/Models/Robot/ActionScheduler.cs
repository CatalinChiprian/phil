using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PHIL_GUI.Models
{
    public class ActionScheduler
    {
        private int nextTempId = -1;
        public ObservableCollection<ScheduledAction> Actions { get; } = new ObservableCollection<ScheduledAction>();
        public Dictionary<ScheduledAction, Well> WellActions { get; } = new Dictionary<ScheduledAction, Well>();
        public ActionScheduler() { }

        public void CreateAction(ActionItem action)
        {
            if (action == null) return;
            ScheduledAction scheduledAction = new ScheduledAction(action);
            if (Actions.Contains(scheduledAction)) return;
            if (Actions.Any(a => a.Id == action.Id)) return;

            action.Model = scheduledAction;
            Actions.Add(scheduledAction);
        }
        public void UpdateAction(ActionItem action)
        {
            if (action == null) return;
            ScheduledAction model = action.Model;
            if (model == null) return;

            model.UpdateFromActionItem(action);

        }
        public void UpdateAction(int tempId, int id)
        {
            ScheduledAction action = Actions.FirstOrDefault(a => a.Id == tempId);
            if (action == null) return;
            int index = Actions.IndexOf(action);
            action.Id = id;
        }
        public void DeleteAction(int actionId)
        { 
            ScheduledAction action = Actions.FirstOrDefault(a => a.Id == actionId);
            if (action == null) return;

            Actions.Remove(action);
        }

        public int GetNextTempId()
        {
            return nextTempId--;
        }

    }
}
