using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHIL_GUI.Models
{
    public class ActionScheduler
    {
        public ObservableCollection<ScheduledAction> Actions { get; } = new ObservableCollection<ScheduledAction>();
        public ActionScheduler() { }
    }
}
