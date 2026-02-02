
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
                (ConnectCommand as RelayCommand)?.RaiseCanExecuteChanged();
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
                (SendMessageCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }
    public object CurrentPage
    {
        get => _currentPage;
        set => SetProperty(ref _currentPage, value);
    }
    
    public ICommand GetPortsCommand { get; }
    public ICommand ConnectCommand { get; }
    public ICommand DisconnectCommand { get; }
    public ICommand SendMessageCommand { get; }
    public ICommand ClearMonitorCommand { get; }
    
    public ICommand GoToBasicControlsPageCommand { get; } 
    
    public ICommand EmergencyStopCommand { get; }
    
    
    
    public MainWindowViewModel()
    {
        _serialPortService = new SerialPortService();
        _availablePorts = new ObservableCollection<string>();
        
        EmergencyStopCommand = new RelayCommand(() => SendMotorCommand("s"));
        GetPortsCommand = new RelayCommand(GetAvailablePorts);
        ConnectCommand = new RelayCommand(ConnectToPort, CanConnect);
        DisconnectCommand = new RelayCommand(DisconnectFromPort, CanDisconnect);
        SendMessageCommand = new RelayCommand(SendMessage, CanSendMessage);
        ClearMonitorCommand = new RelayCommand(ClearMonitor);
        GoToBasicControlsPageCommand = new RelayCommand(GoToBasicControlsPage, CanGoToBasicControlsPage);
        
            
        
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
            
            (ConnectCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (DisconnectCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (SendMessageCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (GoToBasicControlsPageCommand as RelayCommand)?.RaiseCanExecuteChanged();
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
        
        (ConnectCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (DisconnectCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (SendMessageCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (GoToBasicControlsPageCommand as RelayCommand)?.RaiseCanExecuteChanged(); 
        
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
            
            (SendMessageCommand as RelayCommand)?.RaiseCanExecuteChanged();
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