using CommunityToolkit.Mvvm.Input;
using PHIL_GUI.Models;
using PHIL_GUI.ViewModels.Base;
using System.Collections.ObjectModel;
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

        // Used for Front End testing, not actual calibration points
        public ObservableCollection<CalibrationRowItem> CalibrationRows { get; } = new()
        {
            new CalibrationRowItem("A1", "0, 36", 0.5f, 0.8f),
            new CalibrationRowItem("B2", "45, 63", 0.3f, 0.6f),
            new CalibrationRowItem("C3", "18, 0", 0.7f, 0.9f),
            new CalibrationRowItem("D4", "90, 90", 0.2f, 0.4f),
            new CalibrationRowItem("E5", "30, 45", 0.4f, 0.7f),
            new CalibrationRowItem("F6", "60, 30", 0.6f, 0.8f),
            new CalibrationRowItem("G7", "15, 75", 0.5f, 0.9f),
            new CalibrationRowItem("H8", "75, 15", 0.3f, 0.5f),
            new CalibrationRowItem("I9", "90, 0", 0.4f, 0.6f),
            new CalibrationRowItem("J10", "0, 90", 0.2f, 0.4f),
            new CalibrationRowItem("K11", "45, 45", 0.5f, 0.7f),
            new CalibrationRowItem("L12", "30, 60", 0.6f, 0.8f),
            new CalibrationRowItem("M13", "60, 30", 0.4f, 0.6f),
            new CalibrationRowItem("N14", "15, 75", 0.5f, 0.9f),
            new CalibrationRowItem("O15", "75, 15", 0.3f, 0.5f),
            new CalibrationRowItem("P16", "90, 0", 0.4f, 0.6f),
            new CalibrationRowItem("Q17", "0, 90", 2f, 2f),
        };

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
    }
}
