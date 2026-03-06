using PHIL_GUI.Commands;
using PHIL_GUI.ViewModels.Base;
using System.Windows.Input;

namespace PHIL_GUI.ViewModels;

public class BasicControlsViewModel : CommunicationBase
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

        EmergencyStopCommand = new RelayCommand(() => Send("s"));
        MoveLeftCommand = new RelayCommand(() => Send("l"));
        MoveRightCommand = new RelayCommand(() => Send("r"));
        MoveForwardCommand = new RelayCommand(() => Send("f"));
        MoveBackwardCommand = new RelayCommand(() => Send("b"));
        MoveUpCommand = new RelayCommand(() => Send("u"));
        MoveDownCommand = new RelayCommand(() => Send("d"));
    }
}