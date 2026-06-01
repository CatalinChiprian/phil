using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PHIL_GUI.Models
{
    public class WellPlateItemOoC : WellPlateItemBase, IWellPlateItem
    {
        public const int PAIRS_PER_QUADRANT_ROW = 3;
        public const int PAIR_COUNT = 2;
        public ObservableCollection<WellPairItem> Wells { get; } = new();
        public List<WellItem> VisibleWells
        {
            get => visibleWells;
            set => visibleWells = value;
        }

        public List<WellPairItem> SelectedWellPairs
        {
            get
            {
                return Wells.Where(p => p.IsVisible && p.IsSelected).ToList();
            }
        }

        public int SelectedCount => SelectedWellPairs.Count;

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
                    if ((rowIndex / PAIR_COUNT) % PAIR_COUNT != 0) pairIndex -= PAIR_COUNT * PAIRS_PER_QUADRANT_ROW;
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

                if ((pairIndex % PAIRS_PER_QUADRANT_ROW == 0) && (colIndex < ColHeaders.Count)) pairIndex += PAIRS_PER_QUADRANT_ROW;

                pairIndex++;
            }

            visibleWells = Wells
                .Where(p => p.IsVisible)
                .SelectMany(p => new[] { p.In, p.Out })
                .ToList();
        }

        public void Select(string name)
        {
            int pairIndex = int.Parse(name);
            SelectWellPair(pairIndex);
        }

        public void SelectAll()
        {
            foreach (WellPairItem pair in Wells)
            {
                pair.IsSelected = true;
            }
        }

        public void SelectQuadrant(int quadrant)
        {
            SelectQuadrantPairs(quadrant);
        }

        public void Clear()
        {
            foreach (WellPairItem pair in Wells)
            {
                pair.IsSelected = false;
            }
        }

        public List<string> GetSelectedWellNames()
        {
            return SelectedWellPairs.Select(p => p.In.Name).ToList();
        }

        public List<string> GetSelectedNames()
        {
            return SelectedWellPairs.Select(p => p.PairIndex.ToString()).ToList();
        }

        private void SelectWellPair(int pairIndex)
        {
            foreach (WellPairItem pair in Wells)
            {
                if (pair.PairIndex == pairIndex)
                {
                    if (AllowMultipleSelection && pair.IsSelected) pair.IsSelected = false;
                    else pair.IsSelected = true;
                }
                else
                {
                    if (AllowMultipleSelection) continue;

                    pair.IsSelected = false;
                }
            }
        }

        private void SelectQuadrantPairs(int quadrantIndex)
        {
            int startPairIndex = (quadrantIndex - 1) * PAIRS_PER_QUADRANT_ROW * PAIR_COUNT + 1;
            int endPairIndex = startPairIndex + PAIRS_PER_QUADRANT_ROW * PAIR_COUNT - 1;

            bool allQuadPairsSelected = true;
            foreach (WellPairItem pair in Wells)
            {
                if (pair.PairIndex >= startPairIndex && pair.PairIndex <= endPairIndex)
                {
                    if (!pair.IsSelected) allQuadPairsSelected = false;

                    pair.IsSelected = true;
                }
                else
                {
                    if (AllowMultipleSelection) continue;

                    pair.IsSelected = false;
                }
            }

            if (allQuadPairsSelected)
            {
                DeselectQuadrantPairs(quadrantIndex);
                return;
            }
        }

        private void DeselectQuadrantPairs(int quadrantIndex)
        {
            int startPairIndex = (quadrantIndex - 1) * PAIRS_PER_QUADRANT_ROW * PAIR_COUNT + 1;
            int endPairIndex = startPairIndex + PAIRS_PER_QUADRANT_ROW * PAIR_COUNT - 1;
            foreach (WellPairItem pair in Wells)
            {
                if (pair.PairIndex >= startPairIndex && pair.PairIndex <= endPairIndex)
                {
                    pair.IsSelected = false;
                }
            }
        }
    }
}
