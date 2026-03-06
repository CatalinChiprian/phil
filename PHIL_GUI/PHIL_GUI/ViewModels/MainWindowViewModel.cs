
/* Created by Victoria Shvets
Based on Phillip Dettinger work availible on https://github.com/CSDGroup/PHIL.git */

using System;
using System.Windows.Input;
using Avalonia.Threading;
using PHIL_GUI.Commands;
using PHIL_GUI.Services;

namespace PHIL_GUI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public Action Disconnected;

    private readonly SerialPortService _serialPortService;
    private string _receivedData;
    private string _messageToSend;
    private object _currentPage; 
    
    public string ConnectedPort { get; }

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

    private RelayCommand _sendMessageCommand;
    private RelayCommand _goToBasicControlsViewCommand;
    public ICommand SendMessageCommand => _sendMessageCommand;
    public ICommand GoToBasicControlsViewCommand => _goToBasicControlsViewCommand;
    public ICommand ClearMonitorCommand { get; }
    public ICommand EmergencyStopCommand { get; }
    public ICommand DisconnectCommand { get; }

    public MainWindowViewModel(SerialPortService serialPortService)
    {
        _serialPortService = serialPortService;
        ConnectedPort = serialPortService.PortName;

        _sendMessageCommand = new RelayCommand(SendMessage, CanSendMessage);
        _goToBasicControlsViewCommand = new RelayCommand(GoToBasicControlsView, CanGoToBasicControlsView);

        EmergencyStopCommand = new RelayCommand(() => SendMotorCommand("s"));
        DisconnectCommand = new RelayCommand(Disconnect);
        ClearMonitorCommand = new RelayCommand(ClearMonitor);
        
        ReceivedData = "";
        
        _serialPortService.MessageReceived += OnMessageReceived;
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
    
    private bool CanGoToBasicControlsView()
    {
        return _serialPortService.IsConnected;
    }
    
    public void GoToBasicControlsView()
    {
        CurrentPage = new BasicControlsViewModel(this);
    }
    
    public void GoToWellsView()
    {
        CurrentPage = new WellsViewModel(this);
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

    public void Disconnect()
    {
        _serialPortService.Disconnect();
        Disconnected?.Invoke();
    }
}