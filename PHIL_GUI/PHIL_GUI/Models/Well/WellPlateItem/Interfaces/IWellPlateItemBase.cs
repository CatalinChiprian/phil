using System.Collections.Generic;

namespace PHIL_GUI.Models
{
    /// <summary>
    /// Defines the base contract for well plate items.
    /// Provides common functionality for both 96-well and organ-on-chip plate configurations.
    /// </summary>
    public interface IWellPlateItemBase
    {
        /// <summary>
        /// Gets the column header labels.
        /// </summary>
        List<string> ColHeaders { get; }
        /// <summary>
        /// Gets the row header labels.
        /// </summary>
        List<char> RowHeaders { get; }
        /// <summary>
        /// Gets or sets the plate type.
        /// </summary>
        PlateType PlateType { get; set; }
        /// <summary>
        /// Gets or sets whether multiple wells can be selected simultaneously.
        /// </summary>
        bool AllowMultipleSelection { get; set; }
        /// <summary>
        /// Gets the list of currently selected wells.
        /// </summary>
        List<WellItem> SelectedWellItems { get; }

        /// <summary>
        /// Gets a well by its name.
        /// </summary>
        /// <param name="name">The well name.</param>
        /// <returns>The WellItem if found; otherwise, null.</returns>
        WellItem GetWell(string name);
        /// <summary>
        /// Selects a well by its name.
        /// </summary>
        /// <param name="name">The well name to select.</param>
        /// <returns>The selected WellItem, or null if deselected.</returns>
        WellItem SelectWell(string name);
        /// <summary>
        /// Selects all visible wells.
        /// </summary>
        void SelectAllWells();
        /// <summary>
        /// Clears the selection on all visible wells.
        /// </summary>
        void ClearSelection();
    }
}
