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
    public class CalibrationViewModel : ViewModelBase
    {
        public ICommand MoveForwardCommand { get; }
        public ICommand MoveBackwardCommand { get; }
        public ICommand MoveLeftCommand { get; }
        public ICommand MoveRightCommand { get; }
        public ICommand DecreaseStepSizeCommand { get; }
        public ICommand IncreaseStepSizeCommand { get; }
        public ICommand WellsPositionCommand { get; }
        public ICommand RecordPositionCommand { get; }
        public ICommand SolveMappingCommand { get; }
        public ICommand GoToSelectedWellCommand { get; }
        public ICommand DeleteRecordPositionCommand { get; }
        public ICommand ClearCalibrationCommand { get; }
        public ICommand CancelCommand { get; }
        public bool IsDecreaseStepSizeEnabled => RobotProtocolService.RobotState.Settings.StepSize > 0.1;
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
        public bool SolveEnabled => Calibration.Points.Count >= Calibration.MIN_COUNT;
        public RobotSettings Settings => RobotProtocolService.RobotState.Settings;
        public Calibration Calibration => RobotProtocolService.RobotState.Calibration;
        public Well CurrentWell => RobotProtocolService.RobotState.CurrentWell;
        public AppKeyBindings AppKeyBindings => AppSettingsService.AppSettings.AppKeyBindings;

        private CalibrationPoint? selectedCalibrationPoint;
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

        public bool IsWellMenuVisibile => WellPlate.SelectedWellItems.Count > 0;
        public IWellPlateItemBase WellPlate { get; } = new WellPlateItemOoC(true);
        public WellPlateItemOoC? WellPlateItemOoC => WellPlate as WellPlateItemOoC;

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

        private void Points_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(SolveEnabled));

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (CalibrationPoint point in e.NewItems)
                {
                    UpdateWellClass(point, e.Action);
                }
            }
            
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (CalibrationPoint point in e.OldItems)
                {
                    UpdateWellClass(point, e.Action);
                }
            }
        }

        private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Settings.StepSize))
                OnPropertyChanged(nameof(IsDecreaseStepSizeEnabled));
        }

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

        private void SelectWell(string well)
        {
            WellItem selectedWell = WellPlate.SelectWell(well);
            SelectedCalibrationPoint = selectedWell.Calibration;

            OnPropertyChanged(nameof(IsWellMenuVisibile));
            OnPropertyChanged(nameof(RecordEnabled));
        }

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

        private void SolveMapping()
        {
            if (Calibration.Points.Count < Calibration.MIN_COUNT) return;

            RobotProtocolService.SolveMap();
        }

        private void DeleteRecordPosition()
        {
            if (SelectedCalibrationPoint == null) return;

            SelectedCalibrationPoint.ErrorLeft = null;
            SelectedCalibrationPoint.ErrorRight = null;

            string wellName = WellPlate.SelectedWellItems.First().Name;

            RobotProtocolService.DeleteCalibrationPoint(SelectedCalibrationPoint.Name);

            RobotProtocolService.RecordCalibrationPoint(wellName);
        }

        private void ClearCalibration()
        {
            RobotProtocolService.ClearCalibration();
            Calibration.Points.Clear();
        }

        private void Cancel()
        {
            SelectedCalibrationPoint = null;

            WellPlate.SelectWell("");

            OnPropertyChanged(nameof(IsWellMenuVisibile));
            OnPropertyChanged(nameof(RecordEnabled));
        }

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
