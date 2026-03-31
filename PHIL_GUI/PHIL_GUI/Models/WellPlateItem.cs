using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

namespace PHIL_GUI.Models
{
    public class WellPlateItem
    {
        const int COLUMN_COUNT = 12;
        const int PAIRS_PER_QUADRANT = 3;
        const int PAIR_COUNT = 2;
        public List<string> ColHeaders { get; } = Enumerable.Range(1, COLUMN_COUNT).Select(i => i.ToString()).ToList();
        public List<char> RowHeaders { get; } = new() { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H' };
        public PlateType PlateType { get; set; }
        public ObservableCollection<WellItem> Wells96 { get; } = new();
        public ObservableCollection<WellPairItem> WellsOoC { get; } = new();

        public WellPlateItem()
        {
            foreach (char row in RowHeaders)
            {
                for (int col = 1; col <= COLUMN_COUNT; col++)
                {
                    Wells96.Add(new WellItem(row, col));
                }
            }

            var wells = new List<WellItem>();

            int colIndex = 1;
            int rowIndex = 0;
            int pairIndex = 1;

            while (rowIndex < RowHeaders.Count)
            {
                if (colIndex > ColHeaders.Count)
                {
                    colIndex = 1;
                    rowIndex += PAIR_COUNT;
                    if ((rowIndex / PAIR_COUNT) % PAIR_COUNT != 0) pairIndex -= PAIR_COUNT * PAIRS_PER_QUADRANT;
                }

                if (rowIndex >= RowHeaders.Count) break;

                char row = RowHeaders[rowIndex];

                WellItem well = new WellItem(row, colIndex);

                int nextRowIndex = rowIndex + 1;
                int nextColumnIndex = colIndex + 1;
                
                char nextRow = RowHeaders[nextRowIndex];

                WellItem nextWell = new WellItem(nextRow, nextColumnIndex);

                WellsOoC.Add(new WellPairItem(pairIndex, well, nextWell));

                colIndex += PAIR_COUNT;

                if ((pairIndex % PAIRS_PER_QUADRANT == 0) && (colIndex < ColHeaders.Count)) pairIndex += PAIRS_PER_QUADRANT;

                pairIndex++;
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
