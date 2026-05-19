using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;

namespace PHIL_GUI.Models
{
    public class WellPlateItem96 : WellPlateItemBase , IWellPlateItem
    {
        public ObservableCollection<WellItem> Wells { get; } = new();
        public List<WellItem> VisibleWells
        {
            get => visibleWells;
            set => visibleWells = value;
        }

        public WellPlateItem96(bool isCalibrationPage = false)
            : base(isCalibrationPage)
        {
            PlateType = PlateType.Well96;

            foreach (var row in RowHeaders)
            {
                for (int col = 1; col <= COLUMN_COUNT; col++)
                {
                    Wells.Add(new WellItem(row, col));
                }
            }

            visibleWells = Wells.Where(w => w.IsVisible).ToList();
        }
    }
}
