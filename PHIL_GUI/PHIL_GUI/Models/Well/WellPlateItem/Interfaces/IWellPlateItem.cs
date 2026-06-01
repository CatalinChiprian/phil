using System.Collections.Generic;

namespace PHIL_GUI.Models
{
    public interface IWellPlateItem : IWellPlateItemBase
    {
        int SelectedCount { get; }
        void Select(string name);
        void SelectAll();
        void SelectQuadrant(int quadrant);
        void Clear();
        List<string> GetSelectedWellNames();
        List<string> GetSelectedNames();
    }
}
