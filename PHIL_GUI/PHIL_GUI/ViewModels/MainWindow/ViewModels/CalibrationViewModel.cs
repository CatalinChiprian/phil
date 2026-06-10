using CommunityToolkit.Mvvm.Input;
using PHIL_GUI.Models;
using PHIL_GUI.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace PHIL_GUI.ViewModels
{
    /// <summary>
    /// ViewModel for the Calibration page, managing robot manual movement and calibration point recording.
    /// Allows users to map well plate positions by recording actual robot coordinates for specific wells.
    /// </summary>
    public class CalibrationViewModel : ViewModelBase
    {
        /// <summary>
        /// Gets the command to move the robot forward (Y-axis positive direction).
        /// </summary>
        public ICommand MoveForwardCommand { get; }

        /// <summary>
        /// Gets the command to move the robot backward (Y-axis negative direction).
        /// </summary>
        public ICommand MoveBackwardCommand { get; }

        /// <summary>
        /// Gets the command to move the robot left (X-axis negative direction).
        /// </summary>
        public ICommand MoveLeftCommand { get; }

        /// <summary>
        /// Gets the command to move the robot right (X-axis positive direction).
        /// </summary>
        public ICommand MoveRightCommand { get; }

        /// <summary>
        /// Gets the command to decrease the movement step size.
        /// </summary>
        public ICommand DecreaseStepSizeCommand { get; }

        /// <summary>
        /// Gets the command to increase the movement step size.
        /// </summary>
        public ICommand IncreaseStepSizeCommand { get; }

        /// <summary>
        /// Gets the command to select a specific well on the well plate.
        /// </summary>
        public ICommand WellsPositionCommand { get; }

        /// <summary>
        /// Gets the command to record the current robot position as a calibration point.
        /// </summary>
        public ICommand RecordPositionCommand { get; }

        /// <summary>
        /// Gets the command to solve the calibration mapping using recorded points.
        /// </summary>
        public ICommand SolveMappingCommand { get; }

        /// <summary>
        /// Gets the command to navigate the robot to the selected well.
        /// </summary>
        public ICommand GoToSelectedWellCommand { get; }

        /// <summary>
        /// Gets the command to delete the selected calibration point.
        /// </summary>
        public ICommand DeleteRecordPositionCommand { get; }

        /// <summary>
        /// Gets the command to clear all calibration points.
        /// </summary>
        public ICommand ClearCalibrationCommand { get; }

        /// <summary>
        /// Gets the command to cancel the current well selection.
        /// </summary>
        public ICommand CancelCommand { get; }

        /// <summary>
        /// Gets a value indicating whether the decrease step size command can be executed.
        /// Returns true if the current step size is greater than 0.1.
        /// </summary>
        public bool IsDecreaseStepSizeEnabled => RobotProtocolService.RobotState.Settings.StepSize > 0.1;

        /// <summary>
        /// Gets a value indicating whether a calibration point can be recorded for the currently selected well.
        /// Returns false if no well is selected or if the selected well already has a calibration point.
        /// </summary>
        public bool RecordEnabled
        {
            get
            {
                List<WellItem> selectedwellItems = WellPlate.SelectedWellItems;

                if (selectedwellItems == null || selectedwellItems.Count == 0) return false;

                string wellName = selectedwellItems.First().Name;

                return !Calibration.Points.Select(p => p.Name).Contains(wellName);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the solve mapping command can be executed.
        /// Returns true if the minimum number of calibration points have been recorded.
        /// </summary>
        public bool SolveEnabled => Calibration.Points.Count >= Calibration.MIN_COUNT;

        /// <summary>
        /// Gets the robot's current settings including step size and state.
        /// </summary>
        public RobotSettings Settings => RobotProtocolService.RobotState.Settings;

        /// <summary>
        /// Gets the robot's calibration data including all recorded points.
        /// </summary>
        public Calibration Calibration => RobotProtocolService.RobotState.Calibration;

        /// <summary>
        /// Gets the robot's current well position information.
        /// </summary>
        public Well CurrentWell => RobotProtocolService.RobotState.CurrentWell;

        /// <summary>
        /// Gets the application's key binding configuration for keyboard shortcuts.
        /// </summary>
        public AppKeyBindings AppKeyBindings => AppSettingsService.AppSettings.AppKeyBindings;

        private CalibrationPoint? selectedCalibrationPoint;
        /// <summary>
        /// Gets or sets the currently selected calibration point.
        /// When set, automatically selects the corresponding well on the well plate.
        /// </summary>
        public CalibrationPoint? SelectedCalibrationPoint
        {
            get => selectedCalibrationPoint;
            set
            {
                if (value == selectedCalibrationPoint) return;

                SetProperty(ref selectedCalibrationPoint, value);

                if (value == null) return;

                SelectWell(value.Name);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the well context menu should be visible.
        /// Returns true if at least one well is selected.
        /// </summary>
        public bool IsWellMenuVisibile => WellPlate.SelectedWellItems.Count > 0;

        /// <summary>
        /// Gets the well plate model used for calibration (organ-on-chip format with all wells unlocked).
        /// </summary>
        public IWellPlateItemBase WellPlate { get; } = new WellPlateItemOoC(true);

        /// <summary>
        /// Gets the well plate as an organ-on-chip plate.
        /// </summary>
        public WellPlateItemOoC? WellPlateItemOoC => WellPlate as WellPlateItemOoC;

        /// <summary>
        /// Initializes a new instance of the CalibrationViewModel class.
        /// Sets up commands and subscribes to property change events.
        /// </summary>
        public CalibrationViewModel()
        {
            MoveForwardCommand = new RelayCommand(RobotProtocolService.MoveForward);
            MoveBackwardCommand = new RelayCommand(RobotProtocolService.MoveBackward);
            MoveLeftCommand = new RelayCommand(RobotProtocolService.MoveLeft);
            MoveRightCommand = new RelayCommand(RobotProtocolService.MoveRight);
            DecreaseStepSizeCommand = new RelayCommand(RobotProtocolService.DecreaseStepSize);
            IncreaseStepSizeCommand = new RelayCommand(RobotProtocolService.IncreaseStepSize);
            WellsPositionCommand = new RelayCommand<string>(SelectWell);
            RecordPositionCommand = new RelayCommand(RecordPosition);
            SolveMappingCommand = new RelayCommand(SolveMapping);
            GoToSelectedWellCommand = new RelayCommand(GoToSelectedWell);
            DeleteRecordPositionCommand = new RelayCommand(DeleteRecordPosition);
            ClearCalibrationCommand = new RelayCommand(ClearCalibration);
            CancelCommand = new RelayCommand(Cancel);

            Calibration.Points.CollectionChanged += Points_CollectionChanged;
            Settings.PropertyChanged += Settings_PropertyChanged;
            CurrentWell.PropertyChanged += CurrentWell_PropertyChanged;
        }

        /// <summary>
        /// Handles collection changes in the calibration points list.
        /// Updates the SolveEnabled property and refreshes well visual states.
        /// </summary>
        private void Points_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(SolveEnabled));


            IEnumerable<CalibrationPoint> items = Enumerable.Empty<CalibrationPoint>();

            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
            {
                items = e.NewItems.Cast<CalibrationPoint>();
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
            {
                items = e.OldItems.Cast<CalibrationPoint>();
            }

            foreach (CalibrationPoint point in items)
            {
                UpdateWellClass(point, e.Action);
            }
        }

        /// <summary>
        /// Handles property changes in the robot settings.
        /// Updates the IsDecreaseStepSizeEnabled property when the step size changes.
        /// </summary>
        private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Settings.StepSize))
                OnPropertyChanged(nameof(IsDecreaseStepSizeEnabled));
        }

        /// <summary>
        /// Handles property changes in the current well.
        /// Updates well selection and UI state when the current well changes.
        /// </summary>
        private void CurrentWell_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CurrentWell.Type))
            {
                if (CurrentWell.Type != WellType.Standard)
                {
                    SelectedCalibrationPoint = null;
                }
            }

            if (e.PropertyName == nameof(CurrentWell.Name))
            {
                WellPlate.SelectWell(CurrentWell.Name);

                OnPropertyChanged(nameof(IsWellMenuVisibile));
                OnPropertyChanged(nameof(RecordEnabled));
            }
        }

        /// <summary>
        /// Selects a specific well on the well plate and updates the selected calibration point.
        /// </summary>
        /// <param name="well">The well name to select (e.g., "A1").</param>
        private void SelectWell(string well)
        {
            WellItem selectedWell = WellPlate.SelectWell(well);
            SelectedCalibrationPoint = selectedWell.Calibration;

            OnPropertyChanged(nameof(IsWellMenuVisibile));
            OnPropertyChanged(nameof(RecordEnabled));
        }

        /// <summary>
        /// Navigates the robot to the selected well.
        /// Uses calculated coordinates if the well has been solved, otherwise uses hardcoded coordinates.
        /// </summary>
        private void GoToSelectedWell()
        {
            WellItem selectedWellItem = WellPlate.SelectedWellItems.First();
            bool isSolved = selectedWellItem.Calibration?.IsSolved ?? false;
            string wellName = selectedWellItem.Name;

            if (isSolved)
            {
                RobotProtocolService.MoveToCalculatedWell(wellName);
            }
            else
            {
                RobotProtocolService.MoveToHardcodedWell(wellName);
            }

            CurrentWell.Type = WellType.Standard;
            CurrentWell.Name = wellName;
        }

        /// <summary>
        /// Records the current robot position as a calibration point for the selected well.
        /// </summary>
        private void RecordPosition()
        {
            if (!RecordEnabled) return;

            string wellName = WellPlate.SelectedWellItems.First().Name;

            CalibrationPoint point = new CalibrationPoint(wellName, (int)CurrentWell.X, (int)CurrentWell.Y);
            Calibration.Points.Add(point);
            SelectedCalibrationPoint = point;

            RobotProtocolService.RecordCalibrationPoint(wellName);

            OnPropertyChanged(nameof(RecordEnabled));
        }

        /// <summary>
        /// Solves the calibration mapping using the recorded calibration points.
        /// Requires the minimum number of points to be recorded.
        /// </summary>
        private void SolveMapping()
        {
            if (Calibration.Points.Count < Calibration.MIN_COUNT) return;

            RobotProtocolService.SolveMap();
        }

        /// <summary>
        /// Deletes the selected calibration point and re-records it without solved values.
        /// Clears error values for left and right motors.
        /// </summary>
        private void DeleteRecordPosition()
        {
            if (SelectedCalibrationPoint == null) return;

            SelectedCalibrationPoint.ErrorLeft = null;
            SelectedCalibrationPoint.ErrorRight = null;

            string wellName = WellPlate.SelectedWellItems.First().Name;

            RobotProtocolService.DeleteCalibrationPoint(SelectedCalibrationPoint.Name);

            RobotProtocolService.RecordCalibrationPoint(wellName);
        }

        /// <summary>
        /// Clears all calibration points from the robot and the local collection.
        /// Updates all well visual states to reflect the cleared calibration.
        /// </summary>
        private void ClearCalibration()
        {
            RobotProtocolService.ClearCalibration();

            foreach(CalibrationPoint point in Calibration.Points)
            {
                UpdateWellClass(point, NotifyCollectionChangedAction.Remove);
            }

            Calibration.Points.Clear();
        }

        /// <summary>
        /// Cancels the current well selection and clears the selected calibration point.
        /// </summary>
        private void Cancel()
        {
            SelectedCalibrationPoint = null;

            WellPlate.SelectWell("");

            OnPropertyChanged(nameof(IsWellMenuVisibile));
            OnPropertyChanged(nameof(RecordEnabled));
        }

        /// <summary>
        /// Updates the calibration property of a well item when a calibration point is added or removed.
        /// </summary>
        /// <param name="point">The calibration point being added or removed.</param>
        /// <param name="action">The collection change action (Add or Remove).</param>
        private void UpdateWellClass(CalibrationPoint point, NotifyCollectionChangedAction action)
        {
            WellItem wellItem = WellPlate.GetWell(point.Name);

            if (wellItem == null) return;

            if (action == NotifyCollectionChangedAction.Add) wellItem.Calibration = point;
            else if (action == NotifyCollectionChangedAction.Remove) wellItem.Calibration = null;

            if (wellItem.IsSelected) SelectedCalibrationPoint = point;
        }
    }
}
