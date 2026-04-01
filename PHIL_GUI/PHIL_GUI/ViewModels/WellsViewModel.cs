using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using PHIL_GUI.Models;
using PHIL_GUI.ViewModels.Base;
using System;
using System.Windows.Input;

namespace PHIL_GUI.ViewModels;

public class WellsViewModel : ViewModelBase
{
    public ICommand WellsPositionCommand { get; }

    public Well CurrentWell => RobotProtocol.RobotState.CurrentWell;
    public Settings Settings => RobotProtocol.RobotState.Settings;
    public Position Position => RobotProtocol.RobotState.Position;
    public Calibration Calibration => RobotProtocol.RobotState.Calibration;

    public WellPlateItem WellPlate { get; } = new WellPlateItem();

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
    public string MicrostepsText => $"1/{Microsteps}";
    
    public WellsViewModel()
    {
        WellsPositionCommand = new RelayCommand<string>(w => GoToWell(w));
        Settings.PropertyChanged += Settings_PropertyChanged;
        CurrentWell.PropertyChanged += CurrentWell_PropertyChanged;
    }

    private void Settings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Settings.State))
            OnPropertyChanged(nameof(TopNotificationText));

        if (e.PropertyName == nameof(Settings.SelectedPlateType))
            WellPlate.PlateType = Settings.SelectedPlateType;
    }

    private void CurrentWell_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Well.Type))
        {
            if (CurrentWell.Type != WellType.Standard) WellPlate.SelectWell(string.Empty);
        }
    }

    void GoToWell(string well)
    {
        CurrentWell.Type = WellType.Standard;
        CurrentWell.Name = well;
        Settings.State = MoveState.Moving;
        WellPlate.SelectWell(well);
        RobotProtocol.Send($"q{well.ToLower()}");
    }
}