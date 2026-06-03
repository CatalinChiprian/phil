using CommunityToolkit.Mvvm.Input;
using PHIL_GUI.Models;
using PHIL_GUI.ViewModels.Base;
using System.Windows.Input;

namespace PHIL_GUI.ViewModels
{
    public class DebugViewModel : ViewModelBase, ISettingsPage
    {
        public ICommand ClearLogCommand { get; set; }

        private bool areActionsRecorded;
        public bool AreActionRecorded
        {
            get => areActionsRecorded;
            set => SetProperty(ref areActionsRecorded, value);
        }

        public DebugViewModel()
        {
            ClearLogCommand = new RelayCommand(ClearLog);
            AreActionRecorded = AppSettingsService.AppSettings.AreActionRecorded;
        }

        private void ClearLog()
        {
            RobotProtocolService.ClearReceivedData();
        }

        public void ApplyChanges()
        {
            AppSettingsService.AppSettings.AreActionRecorded = AreActionRecorded;
        }
    }
}
