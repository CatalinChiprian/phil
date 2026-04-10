using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PHIL_GUI.Models
{
    public class WellPlateItemOoC : WellPlateItemBase , IWellPlateItem
    {
        const int PAIRS_PER_QUADRANT = 3;
        const int PAIR_COUNT = 2;
        public ObservableCollection<WellPairItem> Wells { get; } = new();
        public List<WellItem> VisibleWells
        {
            get => visibleWells;
            set => visibleWells = value;
        }

        public WellPlateItemOoC(bool isCalibrationPage = false)
            : base(isCalibrationPage)
        {
            PlateType = PlateType.OrganOnChip;

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

                int rowPairIndex = row / PAIR_COUNT;
                bool isRowPairIndexEven = rowPairIndex % 2 == 0;
                bool isPairIndexEven = pairIndex % 2 == 0;
                bool isVisible = !IsCalibrationPage || (isRowPairIndexEven != isPairIndexEven);

                Wells.Add(new WellPairItem(pairIndex, well, nextWell, isVisible));

                colIndex += PAIR_COUNT;

                if ((pairIndex % PAIRS_PER_QUADRANT == 0) && (colIndex < ColHeaders.Count)) pairIndex += PAIRS_PER_QUADRANT;

                pairIndex++;
            }

            visibleWells = Wells
                .Where(p => p.IsVisible)
                .SelectMany(p => new[] { p.In, p.Out })
                .ToList();
        }

        public WellItem SelectWell(string name)
        {
            WellItem selectedWell = null;

            foreach (WellPairItem pair in Wells)
            {
                List<WellItem> pairWells = new List<WellItem> { pair.In, pair.Out };

                foreach (WellItem well in pairWells)
                {
                    if (well.Name == name)
                    {
                        well.IsSelected = true;

                        selectedWell = well;
                    }
                    else
                    {
                        well.IsSelected = false;
                    }
                }
            }

            return selectedWell;
        }
    }
}
