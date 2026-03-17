using PHIL_GUI.Commands;
using PHIL_GUI.ViewModels.Base;
using System.Windows.Input;

namespace PHIL_GUI.ViewModels;

public class BasicControlsViewModel : ViewModelBase
{
    public ICommand EmergencyStopCommand { get; }
    public ICommand MoveLeftCommand { get; }
    public ICommand MoveRightCommand { get; }
    public ICommand MoveForwardCommand { get; }
    public ICommand MoveBackwardCommand { get; }
    public ICommand MoveUpCommand { get; }
    public ICommand MoveDownCommand { get; }
    
    public BasicControlsViewModel()
    {

        EmergencyStopCommand = new RelayCommand(() => RobotProtocol.Send("s"));
        MoveLeftCommand = new RelayCommand(() => RobotProtocol.Send("l"));
        MoveRightCommand = new RelayCommand(() => RobotProtocol.Send("r"));
        MoveForwardCommand = new RelayCommand(() => RobotProtocol.Send("f"));
        MoveBackwardCommand = new RelayCommand(() => RobotProtocol.Send("b"));
        MoveUpCommand = new RelayCommand(() => RobotProtocol.Send("u"));
        MoveDownCommand = new RelayCommand(() => RobotProtocol.Send("d"));
    }
}