using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PHIL_GUI.Models
{
    public class WellPlateItemBase : IWellPlateItemBase
    {
        protected const int COLUMN_COUNT = 12;
        public List<string> ColHeaders { get; } = Enumerable.Range(1, COLUMN_COUNT).Select(i => i.ToString()).ToList();
        public List<char> RowHeaders { get; } = new() { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H' };
        public PlateType PlateType { get; set; }
        public bool IsCalibrationPage { get; }

        protected List<WellItem> visibleWells;

        public WellItem SelectedWellItem
        {
            get
            {
                return visibleWells.FirstOrDefault(w => w.IsSelected);
            }
        }

        public string SelectedWellName => SelectedWellItem?.Name;
        public WellPlateItemBase(bool isCalibrationPage = false)
        {
            IsCalibrationPage = isCalibrationPage;
        }

        public WellItem GetWell(string name)
        {
            return visibleWells.FirstOrDefault(w => w.Name == name);
        }
    }
}
