using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHIL_GUI.Models
{
    public interface IRecordContext
    {
        bool AreActionRecorded { get; }
    }
}
