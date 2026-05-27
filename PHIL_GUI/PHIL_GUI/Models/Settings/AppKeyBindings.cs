using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PHIL_GUI.Models
{
    public class AppKeyBindings : ObservableObject
    {
        private string goHomeStr = "H";
        public string GoHomeStr
        {
            get => goHomeStr;
            set
            {
                if (value == null) return;
                if (value == goHomeStr) return;

                SetProperty(ref goHomeStr, value);
                OnPropertyChanged(nameof(GoHomeKey));
            }
        }

        public KeyGesture GoHomeKey => KeyGesture.Parse(GoHomeStr);

        private string calibrateHomeStr = "C";
        public string CalibrateHomeStr
        {
            get => calibrateHomeStr;
            set
            {
                if (value == null) return;
                if (value == calibrateHomeStr) return;

                SetProperty(ref calibrateHomeStr, value);
                OnPropertyChanged(nameof(CalibrateHomeKey));
            }
        }
        public KeyGesture CalibrateHomeKey => KeyGesture.Parse(CalibrateHomeStr);

        private string moveUpStr = "Up";
        public string MoveUpStr
        {
            get => moveUpStr;
            set
            {
                if (value == null) return;
                if (value == moveUpStr) return;

                SetProperty(ref moveUpStr, value);
                OnPropertyChanged(nameof(MoveUpKey));
            }
        }
        public KeyGesture MoveUpKey => KeyGesture.Parse(MoveUpStr);

        private string moveDownStr = "Down";
        public string MoveDownStr
        {
            get => moveDownStr;
            set
            {
                if (value == null) return;
                if (value == moveDownStr) return;

                SetProperty(ref moveDownStr, value);
                OnPropertyChanged(nameof(MoveDownKey));
            }
        }
        public KeyGesture MoveDownKey => KeyGesture.Parse(MoveDownStr);

        private string moveLeftStr = "A";
        public string MoveLeftStr
        {
            get => moveLeftStr;
            set
            {
                if (value == null) return;
                if (value == moveLeftStr) return;

                SetProperty(ref moveLeftStr, value);
                OnPropertyChanged(nameof(MoveLeftKey));
            }
        }
        public KeyGesture MoveLeftKey => KeyGesture.Parse(MoveLeftStr);

        private string moveRightStr = "D";
        public string MoveRightStr
        {
            get => moveRightStr;
            set
            {
                if (value == null) return;
                if (value == moveRightStr) return;

                SetProperty(ref moveRightStr, value);
                OnPropertyChanged(nameof(MoveRightKey));
            }
        }
        public KeyGesture MoveRightKey => KeyGesture.Parse(MoveRightStr);

        private string moveForwardStr = "W";
        public string MoveForwardStr
        {
            get => moveForwardStr;
            set
            {
                if (value == null) return;
                if (value == moveForwardStr) return;

                SetProperty(ref moveForwardStr, value);
                OnPropertyChanged(nameof(MoveForwardKey));
            }
        }
        public KeyGesture MoveForwardKey => KeyGesture.Parse(MoveForwardStr);

        private string moveBackwardStr = "S";
        public string MoveBackwardStr
        {
            get => moveBackwardStr;
            set
            {
                if (value == null) return;
                if (value == moveBackwardStr) return;

                SetProperty(ref moveBackwardStr, value);
                OnPropertyChanged(nameof(MoveBackwardKey));
            }
        }
        public KeyGesture MoveBackwardKey => KeyGesture.Parse(MoveBackwardStr);

        private string recordPositionStr = "Z";
        public string RecordPositionStr
        {
            get => recordPositionStr;
            set
            {
                if (value == null) return;
                if (value == recordPositionStr) return;

                SetProperty(ref recordPositionStr, value);
                OnPropertyChanged(nameof(RecordPositionKey));
            }
        }
        public KeyGesture RecordPositionKey => KeyGesture.Parse(RecordPositionStr);

        private string solveMapStr = "M";
        public string SolveMapStr
        {
            get => solveMapStr;
            set
            {
                if (value == null) return;
                if (value == solveMapStr) return;

                SetProperty(ref solveMapStr, value);
                OnPropertyChanged(nameof(SolveMapKey));
            }
        }
        public KeyGesture SolveMapKey => KeyGesture.Parse(SolveMapStr);

        private string increaseStepStr = "+";
        public string IncreaseStepStr
        {
            get => increaseStepStr;
            set
            {
                if (value == null) return;
                if (value == increaseStepStr) return;

                SetProperty(ref increaseStepStr, value);
                OnPropertyChanged(nameof(IncreaseStepKey));
            }
        }
        public KeyGesture IncreaseStepKey => KeyGesture.Parse(IncreaseStepStr);

        private string decreaseStepStr = "-";
        public string DecreaseStepStr
        {
            get => decreaseStepStr;
            set
            {
                if (value == null) return;
                if (value == decreaseStepStr) return;

                SetProperty(ref decreaseStepStr, value);
                OnPropertyChanged(nameof(DecreaseStepKey));
            }
        }
        public KeyGesture DecreaseStepKey => KeyGesture.Parse(DecreaseStepStr);

        private string pump1InStr = "D1";
        public string Pump1InStr
        {
            get => pump1InStr;
            set
            {
                if (value == null) return;
                if (value == pump1InStr) return;

                SetProperty(ref pump1InStr, value);
                OnPropertyChanged(nameof(Pump1InKey));
            }
        }
        public KeyGesture Pump1InKey => KeyGesture.Parse(Pump1InStr);

        private string pump1OutStr = "Shift+D1";
        public string Pump1OutStr
        {
            get => pump1OutStr;
            set
            {
                if (value == null) return;
                if (value == pump1OutStr) return;

                SetProperty(ref pump1OutStr, value);
                OnPropertyChanged(nameof(Pump1OutKey));
            }
        }
        public KeyGesture Pump1OutKey => KeyGesture.Parse(Pump1OutStr);

        private string pump2InStr = "D2";
        public string Pump2InStr
        {
            get => pump2InStr;
            set
            {
                if (value == null) return;
                if (value == pump2InStr) return;

                SetProperty(ref pump2InStr, value);
                OnPropertyChanged(nameof(Pump2InKey));
            }
        }
        public KeyGesture Pump2InKey => KeyGesture.Parse(Pump2InStr);

        private string pump2OutStr = "Shift+D2";
        public string Pump2OutStr
        {
            get => pump2OutStr;
            set
            {
                if (value == null) return;
                if (value == pump2OutStr) return;

                SetProperty(ref pump2OutStr, value);
                OnPropertyChanged(nameof(Pump2OutKey));
            }
        }
        public KeyGesture Pump2OutKey => KeyGesture.Parse(Pump2OutStr);

        private string pump3InStr = "D3";
        public string Pump3InStr
        {
            get => pump3InStr;
            set
            {
                if (value == null) return;
                if (value == pump3InStr) return;

                SetProperty(ref pump3InStr, value);
                OnPropertyChanged(nameof(Pump3InKey));
            }
        }
        public KeyGesture Pump3InKey => KeyGesture.Parse(Pump3InStr);

        private string pump3OutStr = "Shift+D3";
        public string Pump3OutStr
        {
            get => pump3OutStr;
            set
            {
                if (value == null) return;
                if (value == pump3OutStr) return;

                SetProperty(ref pump3OutStr, value);
                OnPropertyChanged(nameof(Pump3OutKey));
            }
        }
        public KeyGesture Pump3OutKey => KeyGesture.Parse(Pump3OutStr);

        private string pump4InStr = "D4";
        public string Pump4InStr
        {
            get => pump4InStr;
            set
            {
                if (value == null) return;
                if (value == pump4InStr) return;

                SetProperty(ref pump4InStr, value);
                OnPropertyChanged(nameof(Pump4InKey));
            }
        }
        public KeyGesture Pump4InKey => KeyGesture.Parse(Pump4InStr);

        private string pump4OutStr = "Shift+D4";
        public string Pump4OutStr
        {
            get => pump4OutStr;
            set
            {
                if (value == null) return;
                if (value == pump4OutStr) return;

                SetProperty(ref pump4OutStr, value);
                OnPropertyChanged(nameof(Pump4OutKey));
            }
        }
        public KeyGesture Pump4OutKey => KeyGesture.Parse(Pump4OutStr);

        public AppKeyBindings() { }

        public AppKeyBindings(AppKeyBindings other)
        {
            GoHomeStr = other.GoHomeStr;
            CalibrateHomeStr = other.CalibrateHomeStr;
            MoveUpStr = other.MoveUpStr;
            MoveDownStr = other.MoveDownStr;
            MoveLeftStr = other.MoveLeftStr;
            MoveRightStr = other.MoveRightStr;
            MoveForwardStr = other.MoveForwardStr;
            MoveBackwardStr = other.MoveBackwardStr;
            RecordPositionStr = other.RecordPositionStr;
            SolveMapStr = other.SolveMapStr;
            IncreaseStepStr = other.IncreaseStepStr;
            DecreaseStepStr = other.DecreaseStepStr;
            Pump1InStr = other.Pump1InStr;
            Pump1OutStr = other.Pump1OutStr;
            Pump2InStr = other.Pump2InStr;
            Pump2OutStr = other.Pump2OutStr;
            Pump3InStr = other.Pump3InStr;
            Pump3OutStr = other.Pump3OutStr;
            Pump4InStr = other.Pump4InStr;
            Pump4OutStr = other.Pump4OutStr;
        }
    }
}
