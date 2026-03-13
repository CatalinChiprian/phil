using CommunityToolkit.Mvvm.ComponentModel;

namespace PHIL_GUI.Models
{
    public class WellItem : ObservableObject
    {
        public string Name { get; }
        public char Row { get; }
        public int Column { get; }

        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set => SetProperty(ref isSelected, value);
        }

        private bool isVisible;
        public bool IsVisible
        {
            get => isVisible;
            set => SetProperty(ref isVisible, value);
        }
        public WellItem(char row, int column)
        {
            Row = row;
            Column = column;
            Name = $"{row}{column}";
            IsVisible = true;
        }
    }
}
