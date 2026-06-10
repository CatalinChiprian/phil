using PHIL_GUI.Models;
using PHIL_GUI.ViewModels.Base;

namespace PHIL_GUI.ViewModels
{
    /// <summary>
    /// ViewModel for the Controls settings page, managing keyboard shortcut configuration.
    /// Allows users to view and modify key bindings for robot control operations.
    /// </summary>
    public class ControlsViewModel : ViewModelBase, ISettingsPage
    {
        private AppKeyBindings appKeyBindings;
        /// <summary>
        /// Gets or sets the current key bindings being edited.
        /// Changes are not applied until ApplyChanges is called.
        /// </summary>
        public AppKeyBindings AppKeyBindings
        {
            get => appKeyBindings;
            set => SetProperty(ref appKeyBindings, value);
        }

        /// <summary>
        /// Initializes a new instance of the ControlsViewModel class.
        /// Creates a working copy of the current key bindings for editing.
        /// </summary>
        public ControlsViewModel()
        {
            AppKeyBindings = new AppKeyBindings(AppSettingsService.AppSettings.AppKeyBindings);
        }

        /// <summary>
        /// Applies the edited key bindings to the application settings.
        /// </summary>
        public void ApplyChanges()
        {
            AppSettingsService.AppSettings.AppKeyBindings.Override(AppKeyBindings);
        }
    }
}
