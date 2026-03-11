using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using PHIL_GUI.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using RelayCommand = PHIL_GUI.Commands.RelayCommand;

namespace PHIL_GUI.ViewModels;

public class WellsViewModel : CommunicationBase
{ 
    public ICommand EmergencyStopCommand { get; }
    public ICommand GoHomeCommand { get; }
    public ICommand SelectPlateTypeCommand { get; }
    public ICommand WellsPositionCommand { get; }

    public ObservableCollection<string> Wells { get; } = new();
    public int WellsCount { get; set; } = 12;
    public List<string> ColHeaders { get; } = Enumerable.Range(1, 12).Select(i => i.ToString()).ToList();
    public List<string> RowHeaders { get; } = new() { "A", "B", "C", "D", "E", "F", "G", "H" };

    public double RmsL = 0.57;
    public double RmsR = 0.8;
    public double CalCount = 40;
    public double CalMax = 40;
    public int Microsteps = 8;

    public string RmsDisplayText => $"L {RmsL:F2}°  R {RmsR:F2}°";
    public IBrush RmsColor
    {
        get
        {
            double worst = Math.Max(RmsL, RmsR);
            if (worst > 1.5) return Application.Current.Resources["Warn"] as IBrush ?? Brushes.Red;
            if (worst > 1.0) return Application.Current.Resources["Caution"] as IBrush ?? Brushes.Orange;
            return Application.Current.Resources["Accent"] as IBrush ?? Brushes.Green;
        }
    }
    public string CalPointsText => $"{CalCount}/{CalMax}";
    public IBrush CalPointsColor
    {
        get
        {
            if (CalCount < 10) return Application.Current.Resources["Warn"] as IBrush ?? Brushes.Red;
            if (CalCount < 20) return Application.Current.Resources["Caution"] as IBrush ?? Brushes.Orange;
            return Application.Current.Resources["Accent"] as IBrush ?? Brushes.Green;
        }
    }
    public string MicrostepsText => $"1/{Microsteps}";
    
    public WellsViewModel()
    {
        EmergencyStopCommand = new RelayCommand(() => Send("s"));
        GoHomeCommand = new RelayCommand(GoHome);
        WellsPositionCommand = new RelayCommand<string>(w => GoToWell(w));

        foreach (var row in RowHeaders)
        {
            for (int col = 1; col <= WellsCount; col++)
            {
                Wells.Add($"{row}{col}");
            }
        }
    }

    void GoHome()
    {
        RobotState.CurrentWell.IsHome = true;
        RobotState.CurrentWell.Name = "Home";
        Send("h");
    }

    void GoToWell(string well)
    {
        RobotState.CurrentWell.IsHome = false;
        RobotState.CurrentWell.Name = well;
        Send($"w{well.ToLower()}");
    }
}