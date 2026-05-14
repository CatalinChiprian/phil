using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PHIL_GUI.Models
{
    public class AppKeyBindings : ObservableObject
    {
        private string goHomeKey = "H";
        public string GoHomeKey
        {
            get => goHomeKey;
            set
            {
                if (value == null) return;
                if (value == goHomeKey) return;

                SetProperty(ref goHomeKey, value);
            }
        }

        private string calibrateHomeKey = "C";
        public string CalibrateHomeKey
        {
            get => calibrateHomeKey;
            set
            {
                if (value == null) return;
                if (value == calibrateHomeKey) return;

                SetProperty(ref calibrateHomeKey, value);
            }
        }

        private string moveUpKey = "Up";
        public string MoveUpKey
        {
            get => moveUpKey;
            set
            {
                if (value == null) return;
                if (value == moveUpKey) return;

                SetProperty(ref moveUpKey, value);
            }
        }

        private string moveDownKey = "Down";
        public string MoveDownKey
        {
            get => moveDownKey;
            set
            {
                if (value == null) return;
                if (value == moveDownKey) return;

                SetProperty(ref moveDownKey, value);
            }
        }

        private string moveLeftKey = "A";
        public string MoveLeftKey
        {
            get => moveLeftKey;
            set
            {
                if (value == null) return;
                if (value == moveLeftKey) return;

                SetProperty(ref moveLeftKey, value);
            }
        }

        private string moveRightKey = "D";
        public string MoveRightKey
        {
            get => moveRightKey;
            set
            {
                if (value == null) return;
                if (value == moveRightKey) return;

                SetProperty(ref moveRightKey, value);
            }
        }

        private string moveForwardKey = "W";
        public string MoveForwardKey
        {
            get => moveForwardKey;
            set
            {
                if (value == null) return;
                if (value == moveForwardKey) return;

                SetProperty(ref moveForwardKey, value);
            }
        }

        private string moveBackwardKey = "S";
        public string MoveBackwardKey
        {
            get => moveBackwardKey;
            set
            {
                if (value == null) return;
                if (value == moveBackwardKey) return;

                SetProperty(ref moveBackwardKey, value);
            }
        }

        private string recordPositionKey = "Z";
        public string RecordPositionKey
        {
            get => recordPositionKey;
            set
            {
                if (value == null) return;
                if (value == recordPositionKey) return;

                SetProperty(ref recordPositionKey, value);
            }
        }

        private string solveMapKey = "M";
        public string SolveMapKey
        {
            get => solveMapKey;
            set
            {
                if (value == null) return;
                if (value == solveMapKey) return;

                SetProperty(ref solveMapKey, value);
            }
        }

        private string increaseStepKey = "+";
        public string IncreaseStepKey
        {
            get => increaseStepKey;
            set
            {
                if (value == null) return;
                if (value == increaseStepKey) return;

                SetProperty(ref increaseStepKey, value);
            }
        }

        private string decreaseStepKey = "-";
        public string DecreaseStepKey
        {
            get => decreaseStepKey;
            set
            {
                if (value == null) return;
                if (value == decreaseStepKey) return;

                SetProperty(ref decreaseStepKey, value);
            }
        }

        private string pump1InKey = "D1";
        public string Pump1InKey
        {
            get => pump1InKey;
            set
            {
                if (value == null) return;
                if (value == pump1InKey) return;

                SetProperty(ref pump1InKey, value);
            }
        }

        private string pump1OutKey = "Shift+D1";
        public string Pump1OutKey
        {
            get => pump1OutKey;
            set
            {
                if (value == null) return;
                if (value == pump1OutKey) return;

                SetProperty(ref pump1OutKey, value);
            }
        }

        private string pump2InKey = "D2";
        public string Pump2InKey
        {
            get => pump2InKey;
            set
            {
                if (value == null) return;
                if (value == pump2InKey) return;

                SetProperty(ref pump2InKey, value);
            }
        }

        private string pump2OutKey = "Shift+D2";
        public string Pump2OutKey
        {
            get => pump2OutKey;
            set
            {
                if (value == null) return;
                if (value == pump2OutKey) return;

                SetProperty(ref pump2OutKey, value);
            }
        }

        private string pump3InKey = "D3";
        public string Pump3InKey
        {
            get => pump3InKey;
            set
            {
                if (value == null) return;
                if (value == pump3InKey) return;

                SetProperty(ref pump3InKey, value);
            }
        }

        private string pump3OutKey = "Shift+D3";
        public string Pump3OutKey
        {
            get => pump3OutKey;
            set
            {
                if (value == null) return;
                if (value == pump3OutKey) return;

                SetProperty(ref pump3OutKey, value);
            }
        }

        private string pump4InKey = "D4";
        public string Pump4InKey
        {
            get => pump4InKey;
            set
            {
                if (value == null) return;
                if (value == pump4InKey) return;

                SetProperty(ref pump4InKey, value);
            }
        }

        private string pump4OutKey = "Shift+D4";
        public string Pump4OutKey
        {
            get => pump4OutKey;
            set
            {
                if (value == null) return;
                if (value == pump4OutKey) return;

                SetProperty(ref pump4OutKey, value);
            }
        }

        public AppKeyBindings() { }

        public AppKeyBindings(AppKeyBindings other)
        {
            GoHomeKey = other.GoHomeKey;
            CalibrateHomeKey = other.CalibrateHomeKey;
            MoveUpKey = other.MoveUpKey;
            MoveDownKey = other.MoveDownKey;
            MoveLeftKey = other.MoveLeftKey;
            MoveRightKey = other.MoveRightKey;
            MoveForwardKey = other.MoveForwardKey;
            MoveBackwardKey = other.MoveBackwardKey;
            RecordPositionKey = other.RecordPositionKey;
            SolveMapKey = other.SolveMapKey;
            IncreaseStepKey = other.IncreaseStepKey;
            DecreaseStepKey = other.DecreaseStepKey;
            Pump1InKey = other.Pump1InKey;
            Pump1OutKey = other.Pump1OutKey;
            Pump2InKey = other.Pump2InKey;
            Pump2OutKey = other.Pump2OutKey;
            Pump3InKey = other.Pump3InKey;
            Pump3OutKey = other.Pump3OutKey;
            Pump4InKey = other.Pump4InKey;
            Pump4OutKey = other.Pump4OutKey;
        }
    }
}
