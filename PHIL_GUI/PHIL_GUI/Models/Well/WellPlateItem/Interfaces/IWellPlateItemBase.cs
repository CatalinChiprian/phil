using System.Collections.Generic;

namespace PHIL_GUI.Models
{
    public interface IWellPlateItemBase
    {
        List<string> ColHeaders { get; }
        List<char> RowHeaders { get; }
        PlateType PlateType { get; set; }
        bool AllowMultipleSelection { get; set; }
        List<WellItem> SelectedWellItems { get; }

        WellItem GetWell(string name);
    }
}
