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

namespace PHIL_GUI.ViewModels;

public class WellsViewModel : CommunicationBase
{
    const int WELLSCOUNT = 12;
    public List<string> ColHeaders { get; } = Enumerable.Range(1, WELLSCOUNT).Select(i => i.ToString()).ToList();
    public List<char> RowHeaders { get; } = new() { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H' };

    public ICommand WellsPositionCommand { get; }

    public ObservableCollection<WellItem> Wells { get; } = new ObservableCollection<WellItem>();

    //Take this from RobotState when implemented
    public double RmsL = 0.57;
    public double RmsR = 0.8;
    public int Microsteps = 8;

    public string TopNotificationText
    {
        get
        {
            if (RobotState.Settings.State == MoveState.EmergencyStopped)
                return $"Emergency stop - L: {RobotState.Position.L}, R: {RobotState.Position.R}";

            if (RobotState.Settings.State == MoveState.Moving)
                return $"Moving to {RobotState.CurrentWell.Name}...";

            if (RobotState.CurrentWell.Type == WellType.Standard)
                return $"Moved to {RobotState.CurrentWell.Name} (L: {RobotState.CurrentWell.AngleL}°, R: {RobotState.CurrentWell.AngleR}°)";

            if (RobotState.CurrentWell.Type == WellType.Home)
                return $"Moved to Home (L: {RobotState.Position.L}, R: {RobotState.Position.R})";

            return $"Stopped - L: {RobotState.Position.L}, R: {RobotState.Position.R}";
        }
    }

    public string WellTypeText
    {
        get
        {
            if (RobotState.Settings.Is96Well) return "96-WELL PLATE";
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
        WellsPositionCommand = new RelayCommand<string>(w => GoToWell(w));
        RobotState.Settings.PropertyChanged += Settings_PropertyChanged;
        RobotState.CurrentWell.PropertyChanged += CurrentWell_PropertyChanged;

        foreach (char row in RowHeaders)
        {
            for (int col = 1; col <= WELLSCOUNT; col++)
            {
                Wells.Add(new WellItem(row, col));
            }
        }
    }

    private void Settings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Settings.Is96Well))
        {
            OnPropertyChanged(nameof(WellTypeText));
            ChangePlateType();
        }

        if (e.PropertyName == nameof(Settings.State))
            OnPropertyChanged(nameof(TopNotificationText));
    }

    private void CurrentWell_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Well.Type))
        {
            if (RobotState.CurrentWell.Type != WellType.Standard) SelectWell(string.Empty);
        }
    }

    void GoToWell(string well)
    {
        RobotState.CurrentWell.Type = WellType.Standard;
        RobotState.CurrentWell.Name = well;
        SelectWell(well);
        Send($"q{well.ToLower()}");
        RobotState.Settings.State = MoveState.Moving;
    }

    private void SelectWell(string name)
    {
        foreach (WellItem well in Wells)
        {
            well.IsSelected = well.Name.Equals(name, StringComparison.OrdinalIgnoreCase);
        }
    }

    private void ChangePlateType()
    {
        foreach (WellItem well in Wells)
        {
            if (RobotState.Settings.Is96Well)
                well.IsVisible = true;
            else
                well.IsVisible = (well.Row % 2 != 0) == (well.Column % 2 != 0);
        }
    }
}