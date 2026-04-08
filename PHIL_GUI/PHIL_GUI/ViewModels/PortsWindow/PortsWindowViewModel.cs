using PHIL_GUI.Commands;
using PHIL_GUI.Services;
using PHIL_GUI.ViewModels.Base;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace PHIL_GUI.ViewModels
{
    public class PortsWindowViewModel : ViewModelBase
    {
        public event Action Connected;

        private readonly SerialPortService serialService;
        public ObservableCollection<string> AvailablePorts { get; private set; } = new ObservableCollection<string>();
        public int WindowHeight => 150 + PortsHeight;
        public int PortsHeight => AvailablePorts.Count * 64;

        private string selectedPort;

        public string SelectedPort
        {
            get => selectedPort;
            set
            {
                if (SetProperty(ref selectedPort, value))
                {
                    connectCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand GetPortsCommand { get; }
        private RelayCommand connectCommand;
        public ICommand ConnectCommand => connectCommand;

        public PortsWindowViewModel(SerialPortService serialService)
        {
            this.serialService = serialService;
            AvailablePorts.CollectionChanged += (_, _) =>
            {
                OnPropertyChanged(nameof(PortsHeight));
                OnPropertyChanged(nameof(WindowHeight));
            };
            GetPortsCommand = new RelayCommand(GetAvailablePorts);
            connectCommand = new RelayCommand(ConnectToSelectedPort, CanConnect);
        }

        private void GetAvailablePorts()
        {
            AvailablePorts.Clear();
            foreach (string port in serialService.GetAvailablePorts())
                AvailablePorts.Add(port);
        }

        private bool CanConnect() => !string.IsNullOrWhiteSpace(SelectedPort) && !serialService.IsConnected;

        private void ConnectToSelectedPort()
        {
            serialService.Connect(SelectedPort);
            Connected?.Invoke();
        }
    }
}