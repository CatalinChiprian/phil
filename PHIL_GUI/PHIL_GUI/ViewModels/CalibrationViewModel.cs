using PHIL_GUI.Models;
using PHIL_GUI.ViewModels.Base;
using System.Collections.ObjectModel;

namespace PHIL_GUI.ViewModels
{
    public class CalibrationViewModel : ViewModelBase
    {
        public ObservableCollection<CalibrationRowItem> CalibrationRows { get; } = new()
        {
            new CalibrationRowItem("A1", "0, 36", 0.5f, 0.8f),
            new CalibrationRowItem("B2", "45, 63", 0.3f, 0.6f),
            new CalibrationRowItem("C3", "18, 0", 0.7f, 0.9f)
        };

        public CalibrationViewModel()
        {
            CalibrationRows.Add(new CalibrationRowItem("D4", "90, 90", 0.2f, 0.4f));
        }
    }
}
