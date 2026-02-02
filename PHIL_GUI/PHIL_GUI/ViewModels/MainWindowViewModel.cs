
/* Created by Victoria Shvets
Based on Phillip Dettinger work availible on https://github.com/CSDGroup/PHIL.git */

using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.Threading;
using PHIL_GUI.Commands;
using PHIL_GUI.Services;

namespace PHIL_GUI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly SerialPortService _serialPortService;
    private ObservableCollection<string> _availablePorts;
    private string _selectedPort;
    private string _connectionStatus;
    private string _receivedData;
    private string _messageToSend;
    private object _currentPage; 
    
    public ObservableCollection<string> AvailablePorts
    {
        get => _availablePorts;
        set => SetProperty(ref _availablePorts, value);
    }
    
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
    
    public string ConnectionStatus
    {
        get => _connectionStatus;
        set => SetProperty(ref _connectionStatus, value);
    }
    
    public string ReceivedData
    {
        get => _receivedData;
        set => SetProperty(ref _receivedData, value);
    }
    
    public string MessageToSend
    {
        get => _messageToSend;
        set
        {
            if (SetProperty(ref _messageToSend, value))
            {
                _sendMessageCommand.RaiseCanExecuteChanged();
            }
        }
    }
    public object CurrentPage
    {
        get => _currentPage;
        set => SetProperty(ref _currentPage, value);
    }

    private RelayCommand _connectCommand;
    private RelayCommand _disconnectCommand;
    private RelayCommand _sendMessageCommand;
    private RelayCommand _goToBasicControlsPageCommand;
    public ICommand ConnectCommand => _connectCommand;
    public ICommand DisconnectCommand => _disconnectCommand;
    public ICommand SendMessageCommand => _sendMessageCommand;
    public ICommand GoToBasicControlsPageCommand => _goToBasicControlsPageCommand;
    public ICommand ClearMonitorCommand { get; }
    public ICommand EmergencyStopCommand { get; }
    public ICommand GetPortsCommand { get; }

    public MainWindowViewModel()
    {
        _serialPortService = new SerialPortService();
        _availablePorts = new ObservableCollection<string>();

        _connectCommand = new RelayCommand(ConnectToPort, CanConnect);
        _disconnectCommand = new RelayCommand(DisconnectFromPort, CanDisconnect);
        _sendMessageCommand = new RelayCommand(SendMessage, CanSendMessage);
        _goToBasicControlsPageCommand = new RelayCommand(GoToBasicControlsPage, CanGoToBasicControlsPage);

        EmergencyStopCommand = new RelayCommand(() => SendMotorCommand("s"));
        GetPortsCommand = new RelayCommand(GetAvailablePorts);
        ClearMonitorCommand = new RelayCommand(ClearMonitor);
        
        ConnectionStatus = "Disconnected";
        ReceivedData = "";
        
        _serialPortService.MessageReceived += OnMessageReceived;
    }
    
    private void GetAvailablePorts()
    {
        AvailablePorts.Clear();
        var ports = _serialPortService.GetAvailablePorts();
        foreach (var port in ports)
        {
            AvailablePorts.Add(port);
        }
    }
    
    private bool CanConnect()
    {
        return !string.IsNullOrEmpty(SelectedPort) && !_serialPortService.IsConnected;
    }
    
    private bool CanDisconnect()
    {
        return _serialPortService.IsConnected;
    }
    
    private void ConnectToPort()
    {
        try
        {
            _serialPortService.Connect(SelectedPort);
            ConnectionStatus = $"Connected to {SelectedPort}";

            _connectCommand.RaiseCanExecuteChanged();
            _disconnectCommand.RaiseCanExecuteChanged();
            _sendMessageCommand.RaiseCanExecuteChanged();
            _goToBasicControlsPageCommand.RaiseCanExecuteChanged();
        }
        catch (Exception ex)
        {
            ConnectionStatus = $"Failed: {ex.Message}";
        }
    }
    
    private void DisconnectFromPort()
    {
        _serialPortService.Disconnect();
        ConnectionStatus = "Disconnected";
        
        _connectCommand.RaiseCanExecuteChanged();
        _disconnectCommand.RaiseCanExecuteChanged();
        _sendMessageCommand.RaiseCanExecuteChanged();
        _goToBasicControlsPageCommand.RaiseCanExecuteChanged();      
    }
    
    private void OnMessageReceived(string message)
    {
        Dispatcher.UIThread.Post(() =>
        {
            ReceivedData += $"{DateTime.Now:HH:mm:ss}: {message}\n";
        });
    }
    
    private bool CanSendMessage()
    {
        return _serialPortService.IsConnected && !string.IsNullOrWhiteSpace(MessageToSend);
    }

    
    private void SendMessage()
    {
        if (!string.IsNullOrWhiteSpace(MessageToSend))
        {
            _serialPortService.SendMessage(MessageToSend);
            
            Dispatcher.UIThread.Post(() =>
            {
                ReceivedData += $"{DateTime.Now:HH:mm:ss} [SENT] {MessageToSend}\n";
            });

            MessageToSend = "";

            _sendMessageCommand.RaiseCanExecuteChanged();
        }
    }
    
    private void ClearMonitor()
    {
        ReceivedData = "";
    }
    
    private bool CanGoToBasicControlsPage()
    {
        return _serialPortService.IsConnected;
    }
    
    public void GoToBasicControlsPage()
    {
        CurrentPage = new BasicControlsViewModel(this);
    }
    
    public void GoToWellsPage()
    {
        CurrentPage = new WellsViewModel(this);
    }
    
    public void GoBackToMainPage()
    {
        CurrentPage = null;
    }
    
    public void SendMotorCommand(string command)
    {
        if (_serialPortService.IsConnected)
        {
            _serialPortService.SendMessage(command);
        
            Dispatcher.UIThread.Post(() =>
            {
                ReceivedData += $"{DateTime.Now:HH:mm:ss} [SENT]: {command}\n";
            });
        }
    }
}