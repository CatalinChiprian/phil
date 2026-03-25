using Avalonia.Input.TextInput;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq;

namespace PHIL_GUI.Models
{
    public class WellPlateItem
    {
        const int WELLSCOUNT = 12;
        public List<string> ColHeaders { get; } = Enumerable.Range(1, WELLSCOUNT).Select(i => i.ToString()).ToList();
        public List<char> RowHeaders { get; } = new() { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H' };
        public PlateType PlateType { get; set; }
        public ObservableCollection<WellItem> Wells96 { get; } = new();
        public ObservableCollection<WellPairItem> WellsOoC { get; } = new();

        public WellPlateItem()
        {
            foreach (char row in RowHeaders)
            {
                for (int col = 1; col <= WELLSCOUNT; col++)
                {
                    Wells96.Add(new WellItem(row, col));
                }
            }

            var wells = new List<WellItem>();

            int colIndex = 1;
            int rowIndex = 0;

            while (rowIndex < RowHeaders.Count)
            {
                if (colIndex > ColHeaders.Count)
                {
                    colIndex = 1;
                    rowIndex += 2;
                }

                if (rowIndex >= RowHeaders.Count) break;

                char row = RowHeaders[rowIndex];

                wells.Add(new WellItem(row, colIndex));

                int nextRowIndex = rowIndex + 1;
                int nextColumnIndex = colIndex + 1;
                
                char nextRow = RowHeaders[nextRowIndex];

                wells.Add(new WellItem(nextRow, nextColumnIndex));

                colIndex += 2;
            }

            for (int i = 0; i < wells.Count; i += 2)
            {
                int pairIndex = (i / 2) + 1;
                WellPairItem pairItem = new WellPairItem(pairIndex, wells[i], wells[i + 1]);
                WellsOoC.Add(pairItem);
            }
        }

        public void SelectWell(string name)
        {
            if (PlateType == PlateType.Well96)
            {
                foreach (WellItem well in Wells96)
                {
                    well.IsSelected = well.Name.Equals(name, StringComparison.OrdinalIgnoreCase);
                }
            }
            else
            {
                foreach (WellPairItem pair in WellsOoC)
                {
                    pair.In.IsSelected = pair.In.Name.Equals(name, StringComparison.OrdinalIgnoreCase);
                    pair.Out.IsSelected = pair.Out.Name.Equals(name, StringComparison.OrdinalIgnoreCase);
                }
            }
        }
    }
}
