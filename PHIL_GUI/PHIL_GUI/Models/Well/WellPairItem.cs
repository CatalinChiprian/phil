using CommunityToolkit.Mvvm.ComponentModel;

namespace PHIL_GUI.Models
{
    public class WellPairItem : ObservableObject
    {
        public WellItem In { get; set; }
        public WellItem Out { get; set; }
        public int PairIndex { get; set; }

        private bool isVisible;
        public bool IsVisible
        {
            get => isVisible;
            set => SetProperty(ref isVisible, value);
        }

        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set => SetProperty(ref isSelected, value);
        }

        public WellPairItem(int pairIndex, WellItem pair1, WellItem pair2, bool isVisible = true)
        {
            In = pair1;
            Out = pair2;
            PairIndex = pairIndex;
            IsVisible = isVisible;
        }
    }
}
