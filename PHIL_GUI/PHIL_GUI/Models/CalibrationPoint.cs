using CommunityToolkit.Mvvm.ComponentModel;

namespace PHIL_GUI.Models
{
    public class CalibrationPoint : ObservableObject
    {
        private string name;
        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        private string x;
        public string X
        {
            get => x;
            set => SetProperty(ref x, value);
        }

        private string y;
        public string Y
        {
            get => y;
            set => SetProperty(ref y, value);
        }

        private string errorLeft;
        public string ErrorLeft
        {
            get => errorLeft;
            set => SetProperty(ref errorLeft, value);
        }

        private string errorRight;
        public string ErrorRight
        {
            get => errorRight;
            set => SetProperty(ref errorRight, value);
        }
    }
}
