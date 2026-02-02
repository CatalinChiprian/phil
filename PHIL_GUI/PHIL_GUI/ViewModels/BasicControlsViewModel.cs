using System.Windows.Input;
using PHIL_GUI.Commands;

namespace PHIL_GUI.ViewModels;

public class BasicControlsViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainViewModel;
    
    public ICommand GoBackCommand { get; }
    public ICommand EmergencyStopCommand { get; }
    public ICommand MoveLeftCommand { get; }
    public ICommand MoveRightCommand { get; }
    public ICommand MoveForwardCommand { get; }
    public ICommand MoveBackwardCommand { get; }
    public ICommand MoveUpCommand { get; }
    public ICommand MoveDownCommand { get; }
    
    public ICommand GoToWellsPageCommand { get; } 
   
    public string ReceivedData => _mainViewModel.ReceivedData;
    public string MessageToSend
    {
        get => _mainViewModel.MessageToSend;
        set => _mainViewModel.MessageToSend = value;
    }
    
    public ICommand SendMessageCommand => _mainViewModel.SendMessageCommand;
    public ICommand ClearMonitorCommand => _mainViewModel.ClearMonitorCommand;
    
    public BasicControlsViewModel(MainWindowViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
        GoBackCommand = new RelayCommand(GoBack);
        GoToWellsPageCommand = new RelayCommand(GoToWellsPage);

        EmergencyStopCommand = new RelayCommand(() => SendMotorCommand("s"));
        MoveLeftCommand = new RelayCommand(() => SendMotorCommand("l"));
        MoveRightCommand = new RelayCommand(() => SendMotorCommand("r"));
        MoveForwardCommand = new RelayCommand(() => SendMotorCommand("f"));
        MoveBackwardCommand = new RelayCommand(() => SendMotorCommand("b"));
        MoveUpCommand = new RelayCommand(() => SendMotorCommand("u"));
        MoveDownCommand = new RelayCommand(() => SendMotorCommand("d"));
        
        _mainViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(_mainViewModel.ReceivedData))
            {
                OnPropertyChanged(nameof(ReceivedData));
            }
        };
    }
    
    private void GoBack()
    {
        _mainViewModel.GoBackToMainPage();
    }
    
    private void GoToWellsPage()
    {
        _mainViewModel.GoToWellsPage();  
    }
    
    private void SendMotorCommand(string command)
    {
        _mainViewModel.SendMotorCommand(command);
    }
}