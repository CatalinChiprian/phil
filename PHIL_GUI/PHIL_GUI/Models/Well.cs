using CommunityToolkit.Mvvm.ComponentModel;

namespace PHIL_GUI.Models
{
    public enum WellType
    {
        Home,
        Standard,
        Unknown
    }
    public class Well : ObservableObject
    {
        private WellType type;
        public WellType Type
        {
            get => type;
            set
            {
                SetProperty(ref type, value);
                if (type == WellType.Home) Name = "Home";
                if (type == WellType.Unknown) Name = "Unknown";
                OnPropertyChanged(nameof(IsStandard));
            }
        }

        public bool IsStandard => Type == WellType.Standard;

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

        private string angleL;
        public string AngleL
        {
            get => angleL;
            set => SetProperty(ref angleL, value);
        }

        private string angleR;
        public string AngleR
        {
            get => angleR;
            set => SetProperty(ref angleR, value);
        }

        public Well()
        {
            Name = "Home";
        }
    }
}
