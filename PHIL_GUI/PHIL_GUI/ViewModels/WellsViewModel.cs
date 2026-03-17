using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using PHIL_GUI.Models;
using PHIL_GUI.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace PHIL_GUI.ViewModels;

public class WellsViewModel : ViewModelBase
{
    const int WELLSCOUNT = 12;
    public List<string> ColHeaders { get; } = Enumerable.Range(1, WELLSCOUNT).Select(i => i.ToString()).ToList();
    public List<char> RowHeaders { get; } = new() { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H' };

    public ICommand WellsPositionCommand { get; }

    public Well CurrentWell => RobotProtocol.RobotState.CurrentWell;
    public Settings Settings => RobotProtocol.RobotState.Settings;
    public Position Position => RobotProtocol.RobotState.Position;
    public Calibration Calibration => RobotProtocol.RobotState.Calibration;

    public ObservableCollection<WellItem> Wells { get; } = new ObservableCollection<WellItem>();

    //Take this from RobotState when implemented
    public double RmsL = 0.57;
    public double RmsR = 0.8;
    public int Microsteps = 8;

    public string TopNotificationText
    {
        get
        {
            if (Settings.State == MoveState.EmergencyStopped)
                return $"Emergency stop - L: {RobotProtocol.RobotState.Position.L}, R: {Position.R}";

            if (Settings.State == MoveState.Moving)
                return $"Moving to {CurrentWell.Name}...";

            if (CurrentWell.Type == WellType.Standard)
                return $"Moved to {CurrentWell.Name} (L: {CurrentWell.AngleL}°, R: {CurrentWell.AngleR}°)";

            if (CurrentWell.Type == WellType.Home)
                return $"Moved to Home (L: {Position.L}, R: {Position.R})";

            return $"Stopped - L: {Position.L}, R: {Position.R}";
        }
    }

    public string WellTypeText
    {
        get
        {
            if (Settings.Is96Well) return "96-WELL PLATE";
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
    public string CalPointsText => $"{Calibration.Count}/{Calibration.MAX_COUNT}";
    public IBrush CalPointsColor
    {
        get
        {
            if (Calibration.Count < 10) return Application.Current.Resources["Warn"] as IBrush;
            if (Calibration.Count < 20) return Application.Current.Resources["Caution"] as IBrush;
            return Application.Current.Resources["Accent"] as IBrush;
        }
    }
    public string MicrostepsText => $"1/{Microsteps}";
    
    public WellsViewModel()
    {
        WellsPositionCommand = new RelayCommand<string>(w => GoToWell(w));
        Settings.PropertyChanged += Settings_PropertyChanged;
        CurrentWell.PropertyChanged += CurrentWell_PropertyChanged;

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
            if (CurrentWell.Type != WellType.Standard) SelectWell(string.Empty);
        }
    }

    void GoToWell(string well)
    {
        CurrentWell.Type = WellType.Standard;
        CurrentWell.Name = well;
        Settings.State = MoveState.Moving;
        SelectWell(well);
        RobotProtocol.Send($"q{well.ToLower()}");
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
            if (Settings.Is96Well)
                well.IsVisible = true;
            else
                well.IsVisible = (well.Row % 2 != 0) == (well.Column % 2 != 0);
        }
    }
}