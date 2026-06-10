namespace PHIL_GUI.Models
{
    /// <summary>
    /// Defines the contract for settings pages that can apply changes.
    /// </summary>
    public interface ISettingsPage
    {
        /// <summary>
        /// Applies the changes made on the settings page.
        /// </summary>
        void ApplyChanges();
    }
}
