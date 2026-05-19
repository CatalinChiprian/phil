using CommunityToolkit.Mvvm.Input;
using PHIL_GUI.Models;
using PHIL_GUI.ViewModels.Base;
using System;
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
        public ICommand SelectWellCommand { get; }
        public ICommand RecordPositionCommand { get; }
        public ICommand SolveMappingCommand { get; }
        public ICommand GoToSelectedWellCommand { get; }
        public ICommand DeleteRecordPositionCommand { get; }
        public ICommand CancelCommand { get; }
        public bool IsDecreaseStepSizeEnabled => RobotProtocolService.RobotState.Settings.StepSize > 0.1;
        public bool RecordEnabled
        {
            get
            {
                if (WellPlate.SelectedWellName == null) return false;

                return !Calibration.Points.Select(p => p.Name).Contains(WellPlate.SelectedWellName);
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

        public bool IsWellMenuVisibile => WellPlate.SelectedWellName != null;
        public IWellPlateItem WellPlate { get; } = new WellPlateItemOoC(true);
        public WellPlateItemOoC? WellPlateItemOoC => WellPlate as WellPlateItemOoC;

        public CalibrationViewModel()
        {
            MoveForwardCommand = new RelayCommand(RobotProtocolService.MoveForward);
            MoveBackwardCommand = new RelayCommand(RobotProtocolService.MoveBackward);
            MoveLeftCommand = new RelayCommand(RobotProtocolService.MoveLeft);
            MoveRightCommand = new RelayCommand(RobotProtocolService.MoveRight);
            DecreaseStepSizeCommand = new RelayCommand(RobotProtocolService.IncreaseStepSize);
            IncreaseStepSizeCommand = new RelayCommand(RobotProtocolService.DecreaseStepSize);
            SelectWellCommand = new RelayCommand<string>(SelectWell);
            RecordPositionCommand = new RelayCommand(RecordPosition);
            SolveMappingCommand = new RelayCommand(SolveMapping);
            GoToSelectedWellCommand = new RelayCommand(GoToSelectedWell);
            DeleteRecordPositionCommand = new RelayCommand(DeleteRecordPosition);
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
                    UpdateWellClass(point);
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
            bool isSolved = WellPlate.SelectedWellItem.Calibration?.IsSolved ?? false;
            string wellName = WellPlate.SelectedWellName;

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

            CalibrationPoint point = new CalibrationPoint(WellPlate.SelectedWellName, (int)CurrentWell.X, (int)CurrentWell.Y);
            Calibration.Points.Add(point);
            SelectedCalibrationPoint = point;

            RobotProtocolService.RecordCalibrationPoint(WellPlate.SelectedWellName);

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

            RobotProtocolService.DeleteCalibrationPoint(SelectedCalibrationPoint.Name);

            RobotProtocolService.RecordCalibrationPoint(WellPlate.SelectedWellName);
        }

        private void Cancel()
        {
            SelectedCalibrationPoint = null;

            WellPlate.SelectWell("");

            OnPropertyChanged(nameof(IsWellMenuVisibile));
            OnPropertyChanged(nameof(RecordEnabled));
        }

        private void UpdateWellClass(CalibrationPoint point)
        {
            WellItem wellItem = WellPlate.GetWell(point.Name);

            wellItem.Calibration = point;

            if (wellItem.IsSelected) SelectedCalibrationPoint = point;
        }
    }
}
