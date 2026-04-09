using CommunityToolkit.Mvvm.Input;
using PHIL_GUI.ViewModels.Base;
using System.Windows.Input;

namespace PHIL_GUI.ViewModels
{
    public class DebugViewModel : ViewModelBase
    {
        public ICommand ClearLogCommand { get; set; }
        public string ReceivedData => RobotProtocol.ReceivedData;

        public DebugViewModel()
        {
            ClearLogCommand = new RelayCommand(ClearLog);
        }

        private void ClearLog()
        {
            RobotProtocol.ClearReceivedData();

            OnPropertyChanged(nameof(ReceivedData));
        }
    }
}
