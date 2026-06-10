using System.Collections.Generic;
using System.Linq;

namespace PHIL_GUI.Models
{
    /// <summary>
    /// Base class for well plate items providing common functionality for both 96-well and organ-on-chip plates.
    /// Manages well selection, headers, and plate configuration.
    /// </summary>
    public class WellPlateItemBase : IWellPlateItemBase
    {
        /// <summary>
        /// Number of columns in the well plate (12 for standard plates).
        /// </summary>
        public const int COLUMN_COUNT = 12;
        /// <summary>
        /// Number of pairs on a chip in an organ-on-chip configuration.
        /// </summary>
        public const int PAIRS_PER_QUADRANT = 6;
        /// <summary>
        /// Gets the column header labels (1-12).
        /// </summary>
        public List<string> ColHeaders { get; } = Enumerable.Range(1, COLUMN_COUNT).Select(i => i.ToString()).ToList();
        /// <summary>
        /// Gets the row header labels (A-H).
        /// </summary>
        public List<char> RowHeaders { get; } = new() { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H' };
        /// <summary>
        /// Gets or sets the plate type (OrganOnChip or Well96).
        /// </summary>
        public PlateType PlateType { get; set; }
        /// <summary>
        /// Gets whether this is a calibration page (affects well visibility).
        /// </summary>
        public bool IsCalibrationPage { get; }
        /// <summary>
        /// Gets or sets whether multiple wells can be selected simultaneously.
        /// </summary>
        public bool AllowMultipleSelection { get; set; }

        protected List<WellItem> visibleWells;

        /// <summary>
        /// Gets the list of currently selected wells.
        /// </summary>
        public List<WellItem> SelectedWellItems
        {
            get
            {
                return visibleWells.Where(w => w.IsSelected).ToList();
            }
        }
        /// <summary>
        /// Initializes a new instance of the WellPlateItemBase class.
        /// </summary>
        /// <param name="isCalibrationPage">Whether this is for a calibration page.</param>
        public WellPlateItemBase(bool isCalibrationPage = false)
        {
            IsCalibrationPage = isCalibrationPage;
        }

        /// <summary>
        /// Gets a well by its name.
        /// </summary>
        /// <param name="name">The well name (e.g., "A1").</param>
        /// <returns>The WellItem if found; otherwise, null.</returns>
        public WellItem GetWell(string name)
        {
            return visibleWells.FirstOrDefault(w => w.Name == name);
        }
        /// <summary>
        /// Selects a well by its name and manages multi-selection behavior.
        /// </summary>
        /// <param name="name">The well name to select.</param>
        /// <returns>The selected WellItem, or null if deselected.</returns>
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

        /// <summary>
        /// Selects all visible wells.
        /// </summary>
        public void SelectAllWells()
        {
            foreach (var well in visibleWells)
            {
                well.IsSelected = true;
            }
        }

        /// <summary>
        /// Clears the selection on all visible wells.
        /// </summary>
        public void ClearSelection()
        {
            foreach (var well in visibleWells)
            {
                well.IsSelected = false;
            }
        }
    }
}
