using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PHIL_GUI.Models
{
    public class WellPlateItem
    {
        const int WELLSCOUNT = 12;
        public List<string> ColHeaders { get; } = Enumerable.Range(1, WELLSCOUNT).Select(i => i.ToString()).ToList();
        public List<char> RowHeaders { get; } = new() { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H' };
        public PlateType PlateType { get; set; }
        public ObservableCollection<WellItem> Wells { get; } = new ObservableCollection<WellItem>();

        public WellPlateItem()
        {
            foreach (char row in RowHeaders)
            {
                for (int col = 1; col <= WELLSCOUNT; col++)
                {
                    Wells.Add(new WellItem(row, col));
                }
            }
        }

        public void SelectWell(string name)
        {
            foreach (WellItem well in Wells)
            {
                well.IsSelected = well.Name.Equals(name, StringComparison.OrdinalIgnoreCase);
            }
        }

        public void ChangePlateType(PlateType plateType)
        {
            PlateType = plateType;

            foreach (WellItem well in Wells)
            {
                well.ChangeWellType(PlateType);
            }
        }
    }
}
