using CommunityToolkit.Mvvm.Input;
using PHIL_GUI.ViewModels.Base;
using System.Collections.ObjectModel;
using System.Windows.Input;
using RelayCommand = PHIL_GUI.Commands.RelayCommand;

namespace PHIL_GUI.ViewModels;

public class WellsViewModel : CommunicationBase
{ 
    public ICommand EmergencyStopCommand { get; }
    public ICommand GoHomeCommand { get; }
    public ICommand WellsPositionCommand { get; }
    public ICommand MoveUpCommand { get; }
    public ICommand MoveDownCommand { get; }

    public ObservableCollection<string> Wells { get; } = new();
    public int WellsCount { get; set; } = 12;

    public string RmsL;
    public string RmsR;
    public int RmsCount { get; set; }
    public string Microsteps;
    
    public WellsViewModel()
    {
        EmergencyStopCommand = new RelayCommand(() => Send("s"));
        GoHomeCommand = new RelayCommand(() => Send("h"));
        MoveUpCommand = new RelayCommand(() => Send("u"));
        MoveDownCommand = new RelayCommand(() => Send("d"));
        WellsPositionCommand = new RelayCommand<string>(w => Send($"w{w?.ToLower()}"));

        var rows = new[] { "A", "B", "C", "D", "E", "F", "G", "H" };

        foreach (var row in rows)
        {
            for (int col = 1; col <= WellsCount; col++)
            {
                Wells.Add($"{row}{col}");
            }
        }
    }
}