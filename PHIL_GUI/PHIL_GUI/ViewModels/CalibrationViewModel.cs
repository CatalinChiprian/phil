using CommunityToolkit.Mvvm.Input;
using PHIL_GUI.Models;
using PHIL_GUI.ViewModels.Base;
using System;
using System.Collections.ObjectModel;
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
        public bool RecordEnabled => WellPlate.SelectedWell != null;
        public bool SolveEnabled => Calibration.Points.Count >= Calibration.MIN_COUNT;
        public Settings Settings => RobotProtocol.RobotState.Settings;
        public Calibration Calibration => RobotProtocol.RobotState.Calibration;

        public WellPlateItem WellPlate { get; } = new WellPlateItem(true);

        public CalibrationViewModel()
        {
            MoveForwardCommand = new RelayCommand(RobotProtocol.MoveForward);
            MoveBackwardCommand = new RelayCommand(RobotProtocol.MoveBackward);
            MoveLeftCommand = new RelayCommand(RobotProtocol.MoveLeft);
            MoveRightCommand = new RelayCommand(RobotProtocol.MoveRight);
            DecreaseStepSize = new RelayCommand(() => RobotProtocol.Send("-"));
            IncreaseStepSize = new RelayCommand(() => RobotProtocol.Send("+"));
            WellsPositionCommand = new RelayCommand<string>(w => GoToWell(w));
            RecordPositionCommand = new RelayCommand(RecordPosition);
            SolveMappingCommand = new RelayCommand(SolveMapping);

            Calibration.Points.CollectionChanged += Points_CollectionChanged; ;
        }

        private void Points_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(SolveEnabled));
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (CalibrationPoint point in e.NewItems)
                {
                    // point is the newly added item
                    UpdateWellClass(point);
                }
            }
        }

        void GoToWell(string well)
        {
            Settings.State = MoveState.Moving;
            WellPlate.SelectWell(well);
            RobotProtocol.Send($"w{well.ToLower()}");
        }

        void RecordPosition()
        {
            if (WellPlate.SelectedWell == null) return;

            RobotProtocol.Send($"z {WellPlate.SelectedWell.Name.ToLower()}");
        }

        void SolveMapping()
        {
            if (Calibration.Points.Count < Calibration.MIN_COUNT) return;

            RobotProtocol.Send("z solve");
        }

        void UpdateWellClass(CalibrationPoint point)
        {
            WellItem wellItem = WellPlate.DisplayedWells.FirstOrDefault(w => w.Name == point.Name);
            wellItem.Calibration = point;
        }
    }
}
