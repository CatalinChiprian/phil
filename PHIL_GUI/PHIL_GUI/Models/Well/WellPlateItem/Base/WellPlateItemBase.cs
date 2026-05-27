using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PHIL_GUI.Models
{
    public class WellPlateItemBase : IWellPlateItemBase
    {
        public const int COLUMN_COUNT = 12;
        public const int ROW_COUNT = 6;
        public List<string> ColHeaders { get; } = Enumerable.Range(1, COLUMN_COUNT).Select(i => i.ToString()).ToList();
        public List<char> RowHeaders { get; } = new() { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H' };
        public PlateType PlateType { get; set; }
        public bool IsCalibrationPage { get; }
        public bool AllowMultipleSelection { get; set; }

        protected List<WellItem> visibleWells;

        public List<WellItem> SelectedWellItems
        {
            get
            {
                return visibleWells.Where(w => w.IsSelected).ToList();
            }
        }
        public WellPlateItemBase(bool isCalibrationPage = false)
        {
            IsCalibrationPage = isCalibrationPage;
        }

        public WellItem GetWell(string name)
        {
            return visibleWells.FirstOrDefault(w => w.Name == name);
        }
        public WellItem SelectWell(string name)
        {
            WellItem selectedWell = null;
            foreach (WellItem well in visibleWells)
            {
                if (well.Name == name)
                {
                    if (AllowMultipleSelection && well.IsSelected)
                    {
                        well.IsSelected = false;
                        return null;
                    }
                    else well.IsSelected = true;

                    selectedWell = well;
                }
                else
                {
                    if (AllowMultipleSelection) continue;

                    well.IsSelected = false;
                }
            }
            return selectedWell;
        }

        public void SelectAllWells()
        {
            foreach (var well in visibleWells)
            {
                well.IsSelected = true;
            }
        }

        public void ClearSelection()
        {
            foreach (var well in visibleWells)
            {
                well.IsSelected = false;
            }
        }
    }
}
