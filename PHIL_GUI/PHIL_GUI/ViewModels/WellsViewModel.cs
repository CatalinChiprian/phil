using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using PHIL_GUI.Models;
using PHIL_GUI.Services;
using PHIL_GUI.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using RelayCommand = PHIL_GUI.Commands.RelayCommand;

namespace PHIL_GUI.ViewModels;

public enum PlateType
{
    Well96,
    OrganOnChip
};

public class WellsViewModel : CommunicationBase
{
    const int WELLSCOUNT = 12;
    public List<string> ColHeaders { get; } = Enumerable.Range(1, WELLSCOUNT).Select(i => i.ToString()).ToList();
    public List<char> RowHeaders { get; } = new() { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H' };

    public ICommand EmergencyStopCommand { get; }
    public ICommand GoHomeCommand { get; }
    public ICommand SelectWell96Command { get; }
    public ICommand SelectOrganOnChipCommand { get; }
    public ICommand WellsPositionCommand { get; }


    private PlateType selectedPlateType;
    public PlateType SelectedPlateType
    {
        get => selectedPlateType;
        set
        {
            SetProperty(ref selectedPlateType, value);

            foreach (var well in Wells)
            {
                if (value == PlateType.Well96)
                    well.IsVisible = true;
                else
                    well.IsVisible = (well.Row % 2 != 0) == (well.Column % 2 != 0);
            }
            OnPropertyChanged(nameof(Is96Well));
            OnPropertyChanged(nameof(WellTypeText));
        }
    }
    public bool Is96Well => SelectedPlateType == PlateType.Well96;
    public ObservableCollection<WellItem> Wells { get; } = new ObservableCollection<WellItem>();
    private WellItem activeWell;
    public WellItem ActiveWell
    {
        get => activeWell;
        private set => activeWell = value;
    }

    //Take this from RobotState when implemented
    public double RmsL = 0.57;
    public double RmsR = 0.8;
    public int Microsteps = 8;

    public string TopNotificationText
    {
        get
        {
            if (RobotState.State == MoveState.EmergencyStopped)
                return $"Emergency stop — L: {RobotState.Position.L}, R: {RobotState.Position.R}";

            if (RobotState.State == MoveState.Moving)
                return $"Moving to {RobotState.CurrentWell.Name}...";

            if (RobotState.CurrentWell.Type == WellType.Standard)
                return $"Moved to {RobotState.CurrentWell.Name} (L: {RobotState.CurrentWell.AngleL}°, R: {RobotState.CurrentWell.AngleR}°)";

            if (RobotState.CurrentWell.Type == WellType.Home)
                return $"Moved to Home (L: {RobotState.Position.L}, R: {RobotState.Position.R})";

            return $"Stopped — L: {RobotState.Position.L}, R: {RobotState.Position.R}";
        }
    }

    public string WellTypeText
    {
        get
        {
            if (Is96Well) return "96-WELL PLATE";
            else return "ORGAN-ON-CHIP PLATE";
        }
    }
    public string RmsDisplayText => $"L {RmsL:F2}°  R {RmsR:F2}°";
    public IBrush RmsColor
    {
        get
        {
            double worst = Math.Max(RmsL, RmsR);
            if (worst > 1.5) return Application.Current.Resources["Warn"] as IBrush;
            if (worst > 1.0) return Application.Current.Resources["Caution"] as IBrush;
            return Application.Current.Resources["Accent"] as IBrush;
        }
    }
    public string CalPointsText => $"{RobotState.Calibration.Count}/{Calibration.MaxCount}";
    public IBrush CalPointsColor
    {
        get
        {
            if (RobotState.Calibration.Count < 10) return Application.Current.Resources["Warn"] as IBrush;
            if (RobotState.Calibration.Count < 20) return Application.Current.Resources["Caution"] as IBrush;
            return Application.Current.Resources["Accent"] as IBrush;
        }
    }
    public string MicrostepsText => $"1/{Microsteps}";
    
    public WellsViewModel()
    {
        EmergencyStopCommand = new RelayCommand(Stop);
        GoHomeCommand = new RelayCommand(GoHome);
        WellsPositionCommand = new RelayCommand<string>(w => GoToWell(w));
        SelectWell96Command = new RelayCommand(() => SelectedPlateType = PlateType.Well96);
        SelectOrganOnChipCommand = new RelayCommand(() => SelectedPlateType = PlateType.OrganOnChip);

        RobotState.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(RobotStateService.State))
                OnPropertyChanged(nameof(TopNotificationText));
        };

        foreach (var row in RowHeaders)
        {
            for (int col = 1; col <= WELLSCOUNT; col++)
            {
                Wells.Add(new WellItem(row, col));
            }
        }
    }

    void GoHome()
    {
        RobotState.CurrentWell.Type = WellType.Home;
        RobotState.CurrentWell.Name = "Home";
        if (activeWell != null) activeWell.IsSelected = false;
        Send("h");
        RobotState.State = MoveState.Moving;
    }

    void GoToWell(string well)
    {
        RobotState.CurrentWell.Type = WellType.Standard;
        RobotState.CurrentWell.Name = well;
        SelectWell(well);
        Send($"q{well.ToLower()}");
        RobotState.State = MoveState.Moving;
    }

    private void SelectWell(string name)
    {
        if (activeWell != null) activeWell.IsSelected = false;
        var found = Wells.FirstOrDefault(w => w.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (found != null) found.IsSelected = true;

        ActiveWell = found;
    }

    void Stop()
    {
        RobotState.CurrentWell.Name = "-";
        if (activeWell != null) activeWell.IsSelected = false;
        RobotState.CurrentWell.Type = WellType.Unknown;
        RobotState.State = MoveState.EmergencyStopped;
        Send("s");
    }
}