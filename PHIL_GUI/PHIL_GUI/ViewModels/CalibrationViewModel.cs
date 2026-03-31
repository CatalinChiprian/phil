using PHIL_GUI.Models;
using PHIL_GUI.ViewModels.Base;
using System.Collections.ObjectModel;

namespace PHIL_GUI.ViewModels
{
    public class CalibrationViewModel : ViewModelBase
    {
        public Settings Settings => RobotProtocol.RobotState.Settings;

        public WellPlateItem WellPlate { get; } = new WellPlateItem();
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
        }
    }
}
