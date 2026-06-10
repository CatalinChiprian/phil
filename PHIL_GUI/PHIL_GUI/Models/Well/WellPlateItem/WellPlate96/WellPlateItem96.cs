using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;

namespace PHIL_GUI.Models
{
    /// <summary>
    /// Represents a standard 96-well plate configuration (8 rows × 12 columns).
    /// </summary>
    public class WellPlateItem96 : WellPlateItemBase , IWellPlateItem
    {
        /// <summary>
        /// Gets the collection of all wells in the 96-well plate.
        /// </summary>
        public ObservableCollection<WellItem> Wells { get; } = new();
        /// <summary>
        /// Gets or sets the list of visible wells.
        /// </summary>
        public List<WellItem> VisibleWells
        {
            get => visibleWells;
            set => visibleWells = value;
        }

        /// <summary>
        /// Gets the count of currently selected wells.
        /// </summary>
        public int SelectedCount => SelectedWellItems.Count;

        /// <summary>
        /// Initializes a new instance of the WellPlateItem96 class and creates all 96 wells.
        /// </summary>
        /// <param name="isCalibrationPage">Whether this is for a calibration page.</param>
        public WellPlateItem96(bool isCalibrationPage = false)
            : base(isCalibrationPage)
        {
            PlateType = PlateType.Well96;

            foreach (var row in RowHeaders)
            {
                for (int col = 1; col <= COLUMN_COUNT; col++)
                {
                    Wells.Add(new WellItem(row, col));
                }
            }

            visibleWells = Wells.Where(w => w.IsVisible).ToList();
        }

        /// <summary>
        /// Selects a well by its name.
        /// </summary>
        /// <param name="name">The well name.</param>
        public void Select(string name)
        {
            SelectWell(name);
        }

        /// <summary>
        /// Selects all wells in the plate.
        /// </summary>
        public void SelectAll()
        {
            SelectAllWells();
        }

        /// <summary>
        /// Clears all well selections.
        /// </summary>
        public void Clear()
        {
            ClearSelection();
        }

        /// <summary>
        /// Gets the names of all selected wells.
        /// </summary>
        /// <returns>List of selected well names (e.g., "A1", "B3").</returns>
        public List<string> GetSelectedWellNames()
        {
            return SelectedWellItems.Select(w => w.Name).ToList();
        }

        /// <summary>
        /// Gets the names of all selected items (same as GetSelectedWellNames for 96-well plates).
        /// </summary>
        /// <returns>List of selected well names.</returns>
        public List<string> GetSelectedNames()
        {
            return GetSelectedWellNames();
        }

        /// <summary>
        /// Selects a quadrant (not implemented for 96-well plates).
        /// </summary>
        /// <param name="quadrant">The quadrant index.</param>
        public void SelectQuadrant(int quadrant) { }
    }
}
