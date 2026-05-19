using PHIL_GUI.Models;
using PHIL_GUI.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHIL_GUI.ViewModels
{
    public enum ActionWindowMode
    {
        Create,
        Edit
    }
    public class ActionWindowViewModel : ViewModelBase
    {
        public ScheduledAction Action { get; }
        public ActionWindowMode Mode { get; }
        public ActionWindowViewModel(ActionWindowMode mode, ScheduledAction action)
        {
            Mode = mode;
            Action = action;
        }
    }
}
