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
        public ICommand DecreaseStepSize { get; }
        public ICommand IncreaseStepSize { get; }
        public ICommand WellsPositionCommand { get; }
        public ICommand RecordPositionCommand { get; }
        public ICommand SolveMappingCommand { get; }
        public ICommand GoToSelectedWellCommand { get; }
        public ICommand DeleteRecordPositionCommand { get; }
        public ICommand CancelCommand { get; }
        public bool IsDecreaseStepSizeEnabled => RobotProtocol.RobotState.Settings.StepSize > 0.1;
        public bool RecordEnabled
        {
            get
            {
                if (WellPlate.SelectedWellName == null) return false;

                return !Calibration.Points.Select(p => p.Name).Contains(WellPlate.SelectedWellName);
            }
        }
        public bool SolveEnabled => Calibration.Points.Count >= Calibration.MIN_COUNT;
        public Settings Settings => RobotProtocol.RobotState.Settings;
        public Calibration Calibration => RobotProtocol.RobotState.Calibration;
        public Well CurrentWell => RobotProtocol.RobotState.CurrentWell;

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
        public WellPlateItemOoC? WellPlateOoC => WellPlate as WellPlateItemOoC;
        public WellPlateItem96? WellPlateItem96 => WellPlate as WellPlateItem96;

        public CalibrationViewModel()
        {
            MoveForwardCommand = new RelayCommand(RobotProtocol.MoveForward);
            MoveBackwardCommand = new RelayCommand(RobotProtocol.MoveBackward);
            MoveLeftCommand = new RelayCommand(RobotProtocol.MoveLeft);
            MoveRightCommand = new RelayCommand(RobotProtocol.MoveRight);
            DecreaseStepSize = new RelayCommand(() => RobotProtocol.Send("-"));
            IncreaseStepSize = new RelayCommand(() => RobotProtocol.Send("+"));
            WellsPositionCommand = new RelayCommand<string>(w => SelectWell(w));
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

        void SelectWell(string well)
        {
            WellItem selectedWell = WellPlate.SelectWell(well);
            SelectedCalibrationPoint = selectedWell.Calibration;

            OnPropertyChanged(nameof(IsWellMenuVisibile));
            OnPropertyChanged(nameof(RecordEnabled));
        }

        void GoToSelectedWell()
        {
            string moveCmd = "w";

            bool isSolved = WellPlate.SelectedWellItem.Calibration?.IsSolved ?? false;
            string wellName = WellPlate.SelectedWellName;

            if (isSolved) moveCmd = "q";
            RobotProtocol.Send($"{moveCmd}{wellName.ToLower()}");

            CurrentWell.Type = WellType.Standard;
            CurrentWell.Name = wellName;
        }

        void RecordPosition()
        {
            if (!RecordEnabled) return;

            CalibrationPoint point = new CalibrationPoint(WellPlate.SelectedWellName, (int)CurrentWell.X, (int)CurrentWell.Y);
            Calibration.Points.Add(point);
            SelectedCalibrationPoint = point;

            RobotProtocol.Send($"z {WellPlate.SelectedWellName.ToLower()}");

            OnPropertyChanged(nameof(RecordEnabled));
        }

        void SolveMapping()
        {
            if (Calibration.Points.Count < Calibration.MIN_COUNT) return;

            RobotProtocol.Send("z solve");
        }

        void DeleteRecordPosition()
        {
            if (SelectedCalibrationPoint == null) return;

            SelectedCalibrationPoint.ErrorLeft = null;
            SelectedCalibrationPoint.ErrorRight = null;

            RobotProtocol.Send($"z delete {SelectedCalibrationPoint.Name.ToLower()}");

            RobotProtocol.Send($"z {WellPlate.SelectedWellName.ToLower()}");
        }

        void Cancel()
        {
            SelectedCalibrationPoint = null;

            WellPlate.SelectWell("");

            OnPropertyChanged(nameof(IsWellMenuVisibile));
            OnPropertyChanged(nameof(RecordEnabled));
        }

        void UpdateWellClass(CalibrationPoint point)
        {
            WellItem wellItem = WellPlate.GetWell(point.Name);

            wellItem.Calibration = point;

            if (wellItem.IsSelected) SelectedCalibrationPoint = point;
        }
    }
}
