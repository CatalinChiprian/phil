using CommunityToolkit.Mvvm.Input;
using PHIL_GUI.ViewModels.Base;
using System.Windows.Input;

namespace PHIL_GUI.ViewModels
{
    public class DebugViewModel : ViewModelBase
    {
        public ICommand ClearLogCommand { get; set; }
        public string ReceivedData => RobotProtocolService.ReceivedData;

        public DebugViewModel()
        {
            ClearLogCommand = new RelayCommand(ClearLog);
        }

        private void ClearLog()
        {
            RobotProtocolService.ClearReceivedData();

            OnPropertyChanged(nameof(ReceivedData));
        }
    }
}
