using System.Collections.Generic;

namespace PHIL_GUI.Models
{
    public interface IWellPlateItemBase
    {
        List<string> ColHeaders { get; }
        List<char> RowHeaders { get; }
        PlateType PlateType { get; set; }
        WellItem SelectedWellItem { get; }
        string SelectedWellName { get; }

        WellItem GetWell(string name);
    }
}
