using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using PHIL_GUI.Commands;
using PHIL_GUI.Services;

namespace PHIL_GUI.ViewModels
{
    public class PortsViewModel : ViewModelBase
    {
        private readonly SerialPortService _serialService;

        public ObservableCollection<string> AvailablePorts { get; } = new();
        private string _selectedPort;

        public string SelectedPort
        {
            get => _selectedPort;
            set
            {
                if (SetProperty(ref _selectedPort, value))
                {
                    _connectCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand GetPortsCommand { get; }
        private RelayCommand _connectCommand;
        public ICommand ConnectCommand => _connectCommand;

        public event Action? Connected;

        public PortsViewModel(SerialPortService serialService)
        {
            _serialService = serialService;
            GetPortsCommand = new RelayCommand(GetAvailablePorts);
            _connectCommand = new RelayCommand(ConnectToSelectedPort, CanConnect);
        }

        private void GetAvailablePorts()
        {
            AvailablePorts.Clear();
            foreach (var p in _serialService.GetAvailablePorts())
                AvailablePorts.Add(p);
        }

        private bool CanConnect() => !string.IsNullOrWhiteSpace(SelectedPort) && !_serialService.IsConnected;

        private void ConnectToSelectedPort()
        {
            _serialService.Connect(SelectedPort);
            Connected?.Invoke();
        }
    }
}