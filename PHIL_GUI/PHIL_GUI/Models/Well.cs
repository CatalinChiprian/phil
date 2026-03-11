using CommunityToolkit.Mvvm.ComponentModel;

namespace PHIL_GUI.Models
{
    public class Well : ObservableObject
    {
        bool isHome;
        public bool IsHome
        {
            get => isHome;
            set => SetProperty(ref isHome, value);
        }

        string name;
        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        string x;
        public string X
        {
            get => x;
            set => SetProperty(ref x, value);
        }

        string y;
        public string Y
        {
            get => y;
            set => SetProperty(ref y, value);
        }

        string angleL;
        public string AngleL
        {
            get => angleL;
            set => SetProperty(ref angleL, value);
        }

        string angleR;
        public string AngleR
        {
            get => angleR;
            set => SetProperty(ref angleR, value);
        }

        public Well()
        {
            IsHome = true;
            Name = "Home";
        }
    }
}
