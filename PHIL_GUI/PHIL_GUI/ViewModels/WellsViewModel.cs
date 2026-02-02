using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using RelayCommand = PHIL_GUI.Commands.RelayCommand;

namespace PHIL_GUI.ViewModels;

public class WellsViewModel : ViewModelBase, IDisposable
{
    private readonly MainWindowViewModel _mainViewModel;
    private string _statusMessage = "";
    private bool _isStatusVisible = false;
    
    public ICommand GoBackCommand { get; }
    public ICommand EmergencyStopCommand { get; }
    public ICommand GoHomeCommand { get; }
    public ICommand WellsPositionCommand { get; }
    
    public ICommand MoveUpCommand { get; }
    public ICommand MoveDownCommand { get; }
    
    public string ReceivedData => _mainViewModel.ReceivedData;
    
    public string MessageToSend
    {
        get => _mainViewModel.MessageToSend;
        set => _mainViewModel.MessageToSend = value;
    }
    
    
    public ICommand SendMessageCommand => _mainViewModel.SendMessageCommand;
    public ICommand ClearMonitorCommand => _mainViewModel.ClearMonitorCommand;
    
    public WellsViewModel(MainWindowViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
        GoBackCommand = new RelayCommand(GoBack);
        EmergencyStopCommand = new RelayCommand(() => SendMotorCommand("s"));
        GoHomeCommand = new RelayCommand(() => SendMotorCommand("h"));
        MoveUpCommand = new RelayCommand(() => SendMotorCommand("u"));
        MoveDownCommand = new RelayCommand(() => SendMotorCommand("d"));
        
        // Command that accepts the well position as a parameter
        WellsPositionCommand = new RelayCommand<string>(SendWellCommand);

        _mainViewModel.PropertyChanged += OnPropertyChanged;  
    }
    
    private void GoBack()
    {
        Dispose();
        _mainViewModel.GoToBasicControlsPage();  
    }
    
    private void SendMotorCommand(string command)
    {
        _mainViewModel.SendMotorCommand(command);
    }
    
    private void SendWellCommand(string? wellPosition)
    {
        // Convert "A1" to "wa1", "D7" to "wd7", etc.
        string command = $"w{wellPosition?.ToLower()}";
        SendMotorCommand(command);
    }

    private void OnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_mainViewModel.ReceivedData))
        {
            OnPropertyChanged(nameof(ReceivedData));
        }
    }

    public void Dispose()
    {
        if (_mainViewModel == null) return;
        _mainViewModel.PropertyChanged -= OnPropertyChanged;
    }
}