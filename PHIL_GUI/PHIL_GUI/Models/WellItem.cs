using CommunityToolkit.Mvvm.ComponentModel;

namespace PHIL_GUI.Models
{
    public class WellItem : ObservableObject
    {
        public char Row { get; }
        public int Column { get; }

        private string name;
        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

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

        public void ChangeWellType(PlateType plateType)
        {
            if (plateType == PlateType.Well96)
            {
                IsVisible = true;
                Name = $"{Row}{Column}";
            }
            else
            {
                IsVisible = (Row % 2 != 0) == (Column % 2 != 0);
                Name = (Row % 2 != 0) ? "IN" : "OUT";
            }
        }
    }
}
