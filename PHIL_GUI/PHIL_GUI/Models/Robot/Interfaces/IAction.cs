using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHIL_GUI.Models
{
    public interface IAction
    {
        int Id { get; set; }
        ActionType Type { get; set; }
        Pump Pump1 { get; set; }
        Pump Pump2 { get; set; }
        int Amount { get; set; }
        int Frequency { get; set; }
        TimeUnit TimeUnit { get; set; }
        long StartEpoch { get; set; }
        long EndEpoch { get; set; }
    }
}
