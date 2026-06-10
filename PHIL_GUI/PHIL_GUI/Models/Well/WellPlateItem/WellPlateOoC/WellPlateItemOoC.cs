using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PHIL_GUI.Models
{
    /// <summary>
    /// Represents an organ-on-chip (OoC) well plate configuration with paired inlet/outlet wells.
    /// Organizes wells into pairs and quadrants for microfluidic applications.
    /// </summary>
    public class WellPlateItemOoC : WellPlateItemBase, IWellPlateItem
    {
        /// <summary>
        /// Number of well pairs per quadrant row.
        /// </summary>
        public const int PAIRS_PER_QUADRANT_ROW = 3;
        /// <summary>
        /// Number of pair rows in each quadrant.
        /// </summary>
        public const int PAIR_ROW_PER_QUADRANT_COUNT = 2;
        /// <summary>
        /// Gets the collection of all well pairs in the OoC plate.
        /// </summary>
        public ObservableCollection<WellPairItem> Wells { get; } = new();
        /// <summary>
        /// Gets or sets the list of visible individual wells.
        /// </summary>
        public List<WellItem> VisibleWells
        {
            get => visibleWells;
            set => visibleWells = value;
        }

        /// <summary>
        /// Gets the list of currently selected well pairs.
        /// </summary>
        public List<WellPairItem> SelectedWellPairs
        {
            get
            {
                return Wells.Where(p => p.IsVisible && p.IsSelected).ToList();
            }
        }

        /// <summary>
        /// Gets the count of currently selected well pairs.
        /// </summary>
        public int SelectedCount => SelectedWellPairs.Count;

        /// <summary>
        /// Initializes a new instance of the WellPlateItemOoC class and creates all well pairs.
        /// </summary>
        /// <param name="isCalibrationPage">Whether this is for a calibration page (affects visibility).</param>
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
                    rowIndex += PAIR_ROW_PER_QUADRANT_COUNT;
                    if ((rowIndex / PAIR_ROW_PER_QUADRANT_COUNT) % PAIR_ROW_PER_QUADRANT_COUNT != 0) pairIndex -= PAIR_ROW_PER_QUADRANT_COUNT * PAIRS_PER_QUADRANT_ROW;
                }

                if (rowIndex >= RowHeaders.Count) break;

                char row = RowHeaders[rowIndex];

                WellItem well = new WellItem(row, colIndex);

                int nextRowIndex = rowIndex + 1;
                int nextColumnIndex = colIndex + 1;

                char nextRow = RowHeaders[nextRowIndex];

                WellItem nextWell = new WellItem(nextRow, nextColumnIndex);

                int rowPairIndex = row / PAIR_ROW_PER_QUADRANT_COUNT;
                bool isRowPairIndexEven = rowPairIndex % 2 == 0;
                bool isPairIndexEven = pairIndex % 2 == 0;
                bool isVisible = !IsCalibrationPage || (isRowPairIndexEven != isPairIndexEven);

                Wells.Add(new WellPairItem(pairIndex, well, nextWell, isVisible));

                colIndex += PAIR_ROW_PER_QUADRANT_COUNT;

                if ((pairIndex % PAIRS_PER_QUADRANT_ROW == 0) && (colIndex < ColHeaders.Count)) pairIndex += PAIRS_PER_QUADRANT_ROW;

                pairIndex++;
            }

            visibleWells = Wells
                .Where(p => p.IsVisible)
                .SelectMany(p => new[] { p.In, p.Out })
                .ToList();
        }

        /// <summary>
        /// Selects a well pair by its channel/pair number.
        /// </summary>
        /// <param name="name">The pair index as a string.</param>
        public void Select(string name)
        {
            int pairIndex = int.Parse(name);
            SelectWellPair(pairIndex);
        }

        /// <summary>
        /// Selects all well pairs in the plate.
        /// </summary>
        public void SelectAll()
        {
            foreach (WellPairItem pair in Wells)
            {
                pair.IsSelected = true;
            }
        }

        /// <summary>
        /// Selects all well pairs in a specific quadrant.
        /// </summary>
        /// <param name="quadrant">The quadrant number (1-4).</param>
        public void SelectQuadrant(int quadrant)
        {
            SelectQuadrantPairs(quadrant);
        }

        /// <summary>
        /// Clears all well pair selections.
        /// </summary>
        public void Clear()
        {
            foreach (WellPairItem pair in Wells)
            {
                pair.IsSelected = false;
            }
        }

        /// <summary>
        /// Gets the names of the inlet wells from all selected pairs.
        /// </summary>
        /// <returns>List of inlet well names (e.g., "A1", "A3").</returns>
        public List<string> GetSelectedWellNames()
        {
            return SelectedWellPairs.Select(p => p.In.Name).ToList();
        }

        /// <summary>
        /// Gets the pair indices of all selected pairs.
        /// </summary>
        /// <returns>List of pair indices as strings.</returns>
        public List<string> GetSelectedNames()
        {
            return SelectedWellPairs.Select(p => p.PairIndex.ToString()).ToList();
        }

        /// <summary>
        /// Selects a specific well pair by its index and manages multi-selection behavior.
        /// </summary>
        /// <param name="pairIndex">The pair index to select.</param>
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

        /// <summary>
        /// Selects all well pairs within a specific quadrant.
        /// If all pairs in the quadrant are already selected, deselects them instead (toggle behavior).
        /// </summary>
        /// <param name="quadrantIndex">The quadrant index (1-4).</param>
        private void SelectQuadrantPairs(int quadrantIndex)
        {
            int startPairIndex = (quadrantIndex - 1) * PAIRS_PER_QUADRANT_ROW * PAIR_ROW_PER_QUADRANT_COUNT + 1;
            int endPairIndex = startPairIndex + PAIRS_PER_QUADRANT_ROW * PAIR_ROW_PER_QUADRANT_COUNT - 1;

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

        /// <summary>
        /// Deselects all well pairs within a specific quadrant.
        /// </summary>
        /// <param name="quadrantIndex">The quadrant index (1-4).</param>
        private void DeselectQuadrantPairs(int quadrantIndex)
        {
            int startPairIndex = (quadrantIndex - 1) * PAIRS_PER_QUADRANT_ROW * PAIR_ROW_PER_QUADRANT_COUNT + 1;
            int endPairIndex = startPairIndex + PAIRS_PER_QUADRANT_ROW * PAIR_ROW_PER_QUADRANT_COUNT - 1;
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
