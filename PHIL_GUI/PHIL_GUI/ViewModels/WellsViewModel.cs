using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using PHIL_GUI.Services;
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

    //Take this from RobotState when implemented
    public double RmsL = 0.57;
    public double RmsR = 0.8;
    public double CalCount = 40;
    public double CalMax = 40;
    public int Microsteps = 8;

    public string TopNotificationText
    {
        get
        {
            if (RobotState.State == MoveState.EmergencyStopped)
                return $"Emergency stop — L: {RobotState.Position.L}, R: {RobotState.Position.R}";

            if (RobotState.State == MoveState.Moving)
                return $"Moving to {RobotState.CurrentWell.Name}...";

            if (RobotState.CurrentWell.Type == Models.WellType.Standard)
                return $"Moved to {RobotState.CurrentWell.Name} (L: {RobotState.CurrentWell.AngleL}°, R: {RobotState.CurrentWell.AngleR}°)";

            if (RobotState.CurrentWell.Type == Models.WellType.Home)
                return $"Moved to Home (L: {RobotState.Position.L}, R: {RobotState.Position.R})";

            return $"Stopped — L: {RobotState.Position.L}, R: {RobotState.Position.R}";
        }
    }
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
        EmergencyStopCommand = new RelayCommand(Stop);
        GoHomeCommand = new RelayCommand(GoHome);
        WellsPositionCommand = new RelayCommand<string>(w => GoToWell(w));

        RobotState.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(RobotStateService.State))
                OnPropertyChanged(nameof(TopNotificationText));
        };

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
        RobotState.CurrentWell.Type = Models.WellType.Home;
        RobotState.CurrentWell.Name = "Home";
        Send("h");
        RobotState.State = MoveState.Moving;
    }

    void GoToWell(string well)
    {
        RobotState.CurrentWell.Type = Models.WellType.Standard;
        RobotState.CurrentWell.Name = well;
        Send($"w{well.ToLower()}");
        RobotState.State = MoveState.Moving;
    }

    void Stop()
    {
        RobotState.CurrentWell.Name = "-";
        RobotState.CurrentWell.Type = Models.WellType.Unknown;
        RobotState.State = MoveState.EmergencyStopped;
        Send("s");
    }
}