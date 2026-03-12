using CommunityToolkit.Mvvm.ComponentModel;

namespace PHIL_GUI.Models
{
    public class Position : ObservableObject
    {
        private string l;
        public string L
        {
            get => l; 
            set => SetProperty(ref l, value);
        }

        private string r;
        public string R
        {
            get => r; 
            set => SetProperty(ref r, value);
        }

        private string z1;
        public string Z1
        {
            get => z1;
            set => SetProperty(ref z1, value);
        }

        private string z2;
        public string Z2
        {
            get => z2;
            set => SetProperty(ref z2, value);
        }
    }
}
