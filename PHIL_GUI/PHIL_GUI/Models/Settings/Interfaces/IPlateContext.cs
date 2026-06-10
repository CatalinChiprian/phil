namespace PHIL_GUI.Models
{
    /// <summary>
    /// Defines the contract for components that need access to the selected plate type.
    /// </summary>
    public interface IPlateContext
    {
        /// <summary>
        /// Gets the currently selected plate type.
        /// </summary>
        PlateType SelectedPlateType { get; }
    }
}
