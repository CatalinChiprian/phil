using CommunityToolkit.Mvvm.Input;
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
        public ObservableCollection<string> AvailablePorts { get; private set; } = new ObservableCollection<string>();
        public int WindowHeight => 150 + PortsHeight;
        public int PortsHeight => AvailablePorts.Count * 64;

        private string selectedPort;

        public string SelectedPort
        {
            get => selectedPort;
            set
            {
                if (value == selectedPort) return;

                SetProperty(ref selectedPort, value);

                OnPropertyChanged(nameof(CanConnect));
            }
        }

        public bool CanConnect => !string.IsNullOrEmpty(SelectedPort);

        public ICommand GetPortsCommand { get; }
        public ICommand ConnectCommand { get; }

        public PortsWindowViewModel()
        {
            AvailablePorts.CollectionChanged += AvailablePorts_CollectionChanged;
            RobotProtocolService.OnAppInitialized += RobotProtocolService_OnAppInitialized;

            GetPortsCommand = new RelayCommand(GetAvailablePorts);
            ConnectCommand = new RelayCommand(ConnectToSelectedPort);
        }

        private void AvailablePorts_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(PortsHeight));
            OnPropertyChanged(nameof(WindowHeight));
        }

        private void RobotProtocolService_OnAppInitialized()
        {
            Connected?.Invoke();
        }

        private void GetAvailablePorts()
        {
            AvailablePorts.Clear();
            foreach (string port in RobotProtocolService.SerialPortService.GetAvailablePorts())
                AvailablePorts.Add(port);
        }

        private void ConnectToSelectedPort()
        {
            RobotProtocolService.SerialPortService.Connect(SelectedPort);
        }
    }
}