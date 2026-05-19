using CommunityToolkit.Mvvm.Input;
using PHIL_GUI.Models;
using PHIL_GUI.ViewModels.Base;
using System.Windows.Input;

namespace PHIL_GUI.ViewModels
{
    public class WellsViewModel : ViewModelBase
    {
        public ICommand WellsPositionCommand { get; }

        public Well CurrentWell => RobotProtocolService.RobotState.CurrentWell;
        public RobotSettings RobotSettings => RobotProtocolService.RobotState.Settings;
        public AppSettings AppSettings => AppSettingsService.AppSettings;
        public Position Position => RobotProtocolService.RobotState.Position;
        public Calibration Calibration => RobotProtocolService.RobotState.Calibration;

        public IWellPlateItem WellPlate { get; private set; }
        public WellPlateItemOoC? WellPlateItemOoC => WellPlate as WellPlateItemOoC;
        public WellPlateItem96? WellPlateItem96 => WellPlate as WellPlateItem96;

        public string TopNotificationText
        {
            get
            {
                if (RobotSettings.State == MoveState.EmergencyStopped)
                    return $"Emergency stop - L: {RobotProtocolService.RobotState.Position.L}, R: {Position.R}";

                if (RobotSettings.State == MoveState.Moving)
                    return $"Moving to {CurrentWell.Name}...";

                if (CurrentWell.Type == WellType.Standard)
                    return $"Moved to {CurrentWell.Name} (L: {CurrentWell.AngleL}°, R: {CurrentWell.AngleR}°)";

                if (CurrentWell.Type == WellType.Home)
                    return $"Moved to Home (L: {Position.L}, R: {Position.R})";

                return $"Stopped - L: {Position.L}, R: {Position.R}";
            }
        }

        public WellsViewModel()
        {
            WellPlate = AppSettings.Is96Well ? new WellPlateItem96() : new WellPlateItemOoC();

            WellsPositionCommand = new RelayCommand<string>(w => GoToWell(w));
            AppSettings.PropertyChanged += AppSettings_PropertyChanged;
            RobotSettings.PropertyChanged += RobotSettings_PropertyChanged;
            CurrentWell.PropertyChanged += CurrentWell_PropertyChanged;
        }

        private void AppSettings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AppSettings.SelectedPlateType))
                OverrideWellPlate();
        }

        private void RobotSettings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RobotSettings.State))
                OnPropertyChanged(nameof(TopNotificationText));
        }

        private void CurrentWell_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Well.Type))
            {
                if (CurrentWell.Type != WellType.Standard) WellPlate.SelectWell(string.Empty);
            }

            if (e.PropertyName == nameof(CurrentWell.Name))
            {
                WellPlate.SelectWell(CurrentWell.Name);
            }
        }

        private void GoToWell(string well)
        {
            CurrentWell.Type = WellType.Standard;
            CurrentWell.Name = well;
            RobotSettings.State = MoveState.Moving;
            WellPlate.SelectWell(well);
            RobotProtocolService.MoveToCalculatedWell(well);
        }

        private void OverrideWellPlate()
        {
            string selectedWellName = CurrentWell.Name;

            if (AppSettings.Is96Well)
            {
                WellPlate = new WellPlateItem96();

                OnPropertyChanged(nameof(WellPlateItem96));
            }
            else
            {
                WellPlate = new WellPlateItemOoC();

                OnPropertyChanged(nameof(WellPlateItemOoC));
            }

            WellPlate.SelectWell(selectedWellName);
        }
    }
}