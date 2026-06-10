using CommunityToolkit.Mvvm.Input;
using PHIL_GUI.Models;
using PHIL_GUI.ViewModels.Base;
using System.Windows.Input;

namespace PHIL_GUI.ViewModels
{
    /// <summary>
    /// ViewModel for the Debug settings page, providing developer tools and debugging options.
    /// Allows users to view robot communication logs and configure action recording settings.
    /// </summary>
    public class DebugViewModel : ViewModelBase, ISettingsPage
    {
        /// <summary>
        /// Gets or sets the command to clear the robot communication log.
        /// </summary>
        public ICommand ClearLogCommand { get; set; }

        private bool areActionsRecorded;
        /// <summary>
        /// Gets or sets a value indicating whether action executions should be recorded as videos.
        /// </summary>
        public bool AreActionRecorded
        {
            get => areActionsRecorded;
            set => SetProperty(ref areActionsRecorded, value);
        }

        /// <summary>
        /// Initializes a new instance of the DebugViewModel class.
        /// Sets up commands and loads current debug settings.
        /// </summary>
        public DebugViewModel()
        {
            ClearLogCommand = new RelayCommand(ClearLog);
            AreActionRecorded = AppSettingsService.AppSettings.AreActionRecorded;
        }

        /// <summary>
        /// Clears the robot communication log.
        /// </summary>
        private void ClearLog()
        {
            RobotProtocolService.ClearReceivedData();
        }

        /// <summary>
        /// Applies the edited debug settings to the application settings.
        /// </summary>
        public void ApplyChanges()
        {
            AppSettingsService.AppSettings.AreActionRecorded = AreActionRecorded;
        }
    }
}
