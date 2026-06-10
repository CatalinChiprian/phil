using CommunityToolkit.Mvvm.Input;
using PHIL_GUI.Models;
using PHIL_GUI.ViewModels.Base;
using System.Windows.Input;

namespace PHIL_GUI.ViewModels
{
    /// <summary>
    /// ViewModel for the Wells page, managing well plate display and navigation to specific wells.
    /// Supports both 96-well and organ-on-chip plate formats.
    /// </summary>
    public class WellsViewModel : ViewModelBase
    {
        /// <summary>
        /// Gets the command to navigate the robot to a specific well position.
        /// </summary>
        public ICommand WellsPositionCommand { get; }

        /// <summary>
        /// Gets the robot's current well position information.
        /// </summary>
        public Well CurrentWell => RobotProtocolService.RobotState.CurrentWell;

        /// <summary>
        /// Gets the robot's current settings including step size and state.
        /// </summary>
        public RobotSettings RobotSettings => RobotProtocolService.RobotState.Settings;

        /// <summary>
        /// Gets the application settings including plate type selection.
        /// </summary>
        public AppSettings AppSettings => AppSettingsService.AppSettings;

        /// <summary>
        /// Gets the robot's current position in steps.
        /// </summary>
        public Position Position => RobotProtocolService.RobotState.Position;

        /// <summary>
        /// Gets the robot's calibration data.
        /// </summary>
        public Calibration Calibration => RobotProtocolService.RobotState.Calibration;

        /// <summary>
        /// Gets the current well plate model (either 96-well or organ-on-chip).
        /// </summary>
        public IWellPlateItem WellPlate { get; private set; }

        /// <summary>
        /// Gets the well plate as an organ-on-chip plate, or null if it's a 96-well plate.
        /// </summary>
        public WellPlateItemOoC? WellPlateItemOoC => WellPlate as WellPlateItemOoC;

        /// <summary>
        /// Gets the well plate as a 96-well plate, or null if it's an organ-on-chip plate.
        /// </summary>
        public WellPlateItem96? WellPlateItem96 => WellPlate as WellPlateItem96;

        /// <summary>
        /// Gets the notification text to display at the top of the wells page.
        /// Shows current movement state, position, or error information.
        /// </summary>
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

        /// <summary>
        /// Initializes a new instance of the WellsViewModel class.
        /// Sets up the well plate based on current settings and subscribes to property change events.
        /// </summary>
        public WellsViewModel()
        {
            WellPlate = AppSettings.Is96Well ? new WellPlateItem96() : new WellPlateItemOoC();

            WellsPositionCommand = new RelayCommand<string>(w => GoToWell(w));
            AppSettings.PropertyChanged += AppSettings_PropertyChanged;
            RobotSettings.PropertyChanged += RobotSettings_PropertyChanged;
            CurrentWell.PropertyChanged += CurrentWell_PropertyChanged;
        }

        /// <summary>
        /// Handles property changes in the application settings.
        /// Updates the well plate when the selected plate type changes.
        /// </summary>
        private void AppSettings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AppSettings.SelectedPlateType))
                OverrideWellPlate();
        }

        /// <summary>
        /// Handles property changes in the robot settings.
        /// Updates the top notification text when the robot state changes.
        /// </summary>
        private void RobotSettings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RobotSettings.State))
                OnPropertyChanged(nameof(TopNotificationText));
        }

        /// <summary>
        /// Handles property changes in the current well.
        /// Updates the well plate selection when the current well changes.
        /// </summary>
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

        /// <summary>
        /// Navigates the robot to the specified well using calculated coordinates.
        /// Updates the well selection and robot state.
        /// </summary>
        /// <param name="well">The well name to navigate to (e.g., "A1").</param>
        private void GoToWell(string well)
        {
            CurrentWell.Type = WellType.Standard;
            CurrentWell.Name = well;
            RobotSettings.State = MoveState.Moving;
            WellPlate.SelectWell(well);
            RobotProtocolService.MoveToCalculatedWell(well);
        }

        /// <summary>
        /// Replaces the current well plate model based on the selected plate type in settings.
        /// Preserves the currently selected well after switching plates.
        /// </summary>
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