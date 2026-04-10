using CommunityToolkit.Mvvm.ComponentModel;

namespace PHIL_GUI.Models
{
    public class LimitSwitches : ObservableObject
    {
        private bool z1;
        public bool Z1 
        { 
            get => z1;
            set
            {
                SetProperty(ref z1, value);
                OnPropertyChanged(nameof(CanMoveUp));
            }
        }

        private bool z2;
        public bool Z2 
        { 
            get => z2;
            set
            {
                SetProperty(ref z2, value);
                OnPropertyChanged(nameof(CanMoveUp));
            }
        }

        private bool l;
        public bool L 
        { 
            get => l; 
            set => SetProperty(ref l, value);
        }

        private bool r;
        public bool R 
        { 
            get => r; 
            set => SetProperty(ref r, value);
        }

        public bool CanMoveUp => !(Z1 || Z2);
    }
}
