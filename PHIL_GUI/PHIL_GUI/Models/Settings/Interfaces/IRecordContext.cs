namespace PHIL_GUI.Models
{
    /// <summary>
    /// Defines the contract for components that need access to the action recording state.
    /// </summary>
    public interface IRecordContext
    {
        /// <summary>
        /// Gets whether actions will be recorded.
        /// </summary>
        bool AreActionRecorded { get; }
    }
}
