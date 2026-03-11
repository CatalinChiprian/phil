using CommunityToolkit.Mvvm.ComponentModel;
using PHIL_GUI.Models;

namespace PHIL_GUI.Services
{
    public class RobotStateService : ObservableObject
    {
        private Well currentWell = new Well();
        public Well CurrentWell
        {
            get => currentWell;
            set => SetProperty(ref currentWell, value);
        }

        private bool isZLimitReached;
        public bool IsZLimitReached 
        { 
            get => isZLimitReached;
            set => SetProperty(ref isZLimitReached, value);
        }

        private double rmsL, rmsR;
        public double RmsL { get => rmsL; set => SetProperty(ref rmsL, value); }
        public double RmsR { get => rmsR; set => SetProperty(ref rmsR, value); }
    }
}
