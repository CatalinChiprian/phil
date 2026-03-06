using PHIL_GUI.Commands;
using PHIL_GUI.Services;
using PHIL_GUI.ViewModels.Base;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace PHIL_GUI.ViewModels
{
    public class PortsViewModel : ViewModelBase
    {
        public event Action Connected;

        private readonly SerialPortService _serialService;
        public ObservableCollection<string> AvailablePorts { get; private set; } = new ObservableCollection<string>();
        public int WindowHeight => 150 + PortsHeight;
        public int PortsHeight => AvailablePorts.Count * 64;

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

        public PortsViewModel(SerialPortService serialService)
        {
            _serialService = serialService;
            AvailablePorts.CollectionChanged += (_, _) =>
            {
                OnPropertyChanged(nameof(PortsHeight));
                OnPropertyChanged(nameof(WindowHeight));
            };
            GetPortsCommand = new RelayCommand(GetAvailablePorts);
            _connectCommand = new RelayCommand(ConnectToSelectedPort, CanConnect);
        }

        private void GetAvailablePorts()
        {
            AvailablePorts.Clear();
            foreach (string port in _serialService.GetAvailablePorts())
                AvailablePorts.Add(port);
        }

        private bool CanConnect() => !string.IsNullOrWhiteSpace(SelectedPort) && !_serialService.IsConnected;

        private void ConnectToSelectedPort()
        {
            _serialService.Connect(SelectedPort);
            Connected?.Invoke();
        }
    }
}