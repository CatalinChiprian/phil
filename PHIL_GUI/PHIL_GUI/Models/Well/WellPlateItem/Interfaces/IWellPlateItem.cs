using System.Collections.Generic;

namespace PHIL_GUI.Models
{
    /// <summary>
    /// Defines the contract for well plate items with extended selection functionality.
    /// </summary>
    public interface IWellPlateItem : IWellPlateItemBase
    {
        /// <summary>
        /// Gets the count of currently selected items.
        /// </summary>
        int SelectedCount { get; }
        /// <summary>
        /// Selects a well or pair by name.
        /// </summary>
        /// <param name="name">The well or pair name/index.</param>
        void Select(string name);
        /// <summary>
        /// Selects all wells or pairs.
        /// </summary>
        void SelectAll();
        /// <summary>
        /// Selects a quadrant of wells or pairs.
        /// </summary>
        /// <param name="quadrant">The quadrant number.</param>
        void SelectQuadrant(int quadrant);
        /// <summary>
        /// Clears all selections.
        /// </summary>
        void Clear();
        /// <summary>
        /// Gets the names of all selected wells.
        /// </summary>
        /// <returns>List of well names.</returns>
        List<string> GetSelectedWellNames();
        /// <summary>
        /// Gets the names or indices of all selected items.
        /// </summary>
        /// <returns>List of names or indices.</returns>
        List<string> GetSelectedNames();
    }
}
