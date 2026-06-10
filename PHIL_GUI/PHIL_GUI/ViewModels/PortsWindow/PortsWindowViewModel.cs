using CommunityToolkit.Mvvm.Input;
using PHIL_GUI.Services;
using PHIL_GUI.ViewModels.Base;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace PHIL_GUI.ViewModels
{
    /// <summary>
    /// ViewModel for the Ports window, managing serial port discovery and connection.
    /// Allows users to select and connect to available serial ports for robot communication.
    /// </summary>
    public class PortsWindowViewModel : ViewModelBase
    {
        /// <summary>
        /// Event raised when a connection to a serial port is established.
        /// </summary>
        public event Action Connected;

        /// <summary>
        /// Gets the collection of available serial ports.
        /// </summary>
        public ObservableCollection<string> AvailablePorts { get; private set; } = new ObservableCollection<string>();

        /// <summary>
        /// Gets the height of the window, which adjusts based on the number of available ports.
        /// </summary>
        public int WindowHeight => 150 + PortsHeight;

        /// <summary>
        /// Gets the height of the ports list area based on the number of available ports.
        /// </summary>
        public int PortsHeight => AvailablePorts.Count * 64;

        private string selectedPort;

        /// <summary>
        /// Gets or sets the currently selected serial port.
        /// </summary>
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

        /// <summary>
        /// Gets a value indicating whether a connection can be established.
        /// Returns true if a port has been selected.
        /// </summary>
        public bool CanConnect => !string.IsNullOrEmpty(SelectedPort);

        /// <summary>
        /// Gets the command to refresh the list of available serial ports.
        /// </summary>
        public ICommand GetPortsCommand { get; }

        /// <summary>
        /// Gets the command to connect to the selected serial port.
        /// </summary>
        public ICommand ConnectCommand { get; }

        /// <summary>
        /// Initializes a new instance of the PortsWindowViewModel class.
        /// Sets up commands and subscribes to collection change events.
        /// </summary>
        public PortsWindowViewModel()
        {
            AvailablePorts.CollectionChanged += AvailablePorts_CollectionChanged;

            GetPortsCommand = new RelayCommand(GetAvailablePorts);
            ConnectCommand = new RelayCommand(ConnectToSelectedPort);
        }

        /// <summary>
        /// Handles changes to the available ports collection and updates window dimensions.
        /// </summary>
        private void AvailablePorts_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(PortsHeight));
            OnPropertyChanged(nameof(WindowHeight));
        }

        /// <summary>
        /// Refreshes the list of available serial ports from the system.
        /// </summary>
        private void GetAvailablePorts()
        {
            AvailablePorts.Clear();
            foreach (string port in RobotProtocolService.SerialPortService.GetAvailablePorts())
                AvailablePorts.Add(port);
        }

        /// <summary>
        /// Connects to the selected serial port and raises the Connected event.
        /// </summary>
        private void ConnectToSelectedPort()
        {
            RobotProtocolService.SerialPortService.Connect(SelectedPort);
            Connected?.Invoke();
        }
    }
}