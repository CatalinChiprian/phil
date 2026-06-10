using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PHIL_GUI.Models
{
    /// <summary>
    /// Manages all keyboard key bindings for robot control and operations.
    /// Provides configurable shortcuts for movement, calibration, and pump controls.
    /// </summary>
    public class AppKeyBindings : ObservableObject
    {
        private string goHomeStr = "H";
        /// <summary>
        /// Gets or sets the key binding string for the Go Home command.
        /// </summary>
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

        /// <summary>
        /// Gets the key gesture for the Go Home command.
        /// </summary>
        public KeyGesture GoHomeKey => KeyGesture.Parse(GoHomeStr);

        private string calibrateHomeStr = "C";
        /// <summary>
        /// Gets or sets the key binding string for the Calibrate Home command.
        /// </summary>
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
        /// <summary>
        /// Gets the key gesture for the Calibrate Home command.
        /// </summary>
        public KeyGesture CalibrateHomeKey => KeyGesture.Parse(CalibrateHomeStr);

        private string moveUpStr = "Up";
        /// <summary>
        /// Gets or sets the key binding string for moving the robot up (Z-axis).
        /// </summary>
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
        /// <summary>
        /// Gets the key gesture for moving the robot up.
        /// </summary>
        public KeyGesture MoveUpKey => KeyGesture.Parse(MoveUpStr);

        private string moveDownStr = "Down";
        /// <summary>
        /// Gets or sets the key binding string for moving the robot down (Z-axis).
        /// </summary>
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
        /// <summary>
        /// Gets the key gesture for moving the robot down.
        /// </summary>
        public KeyGesture MoveDownKey => KeyGesture.Parse(MoveDownStr);

        private string moveLeftStr = "A";
        /// <summary>
        /// Gets or sets the key binding string for moving the robot left.
        /// </summary>
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
        /// <summary>
        /// Gets the key gesture for moving the robot left.
        /// </summary>
        public KeyGesture MoveLeftKey => KeyGesture.Parse(MoveLeftStr);

        private string moveRightStr = "D";
        /// <summary>
        /// Gets or sets the key binding string for moving the robot right.
        /// </summary>
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
        /// <summary>
        /// Gets the key gesture for moving the robot right.
        /// </summary>
        public KeyGesture MoveRightKey => KeyGesture.Parse(MoveRightStr);

        private string moveForwardStr = "W";
        /// <summary>
        /// Gets or sets the key binding string for moving the robot forward.
        /// </summary>
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
        /// <summary>
        /// Gets the key gesture for moving the robot forward.
        /// </summary>
        public KeyGesture MoveForwardKey => KeyGesture.Parse(MoveForwardStr);

        private string moveBackwardStr = "S";
        /// <summary>
        /// Gets or sets the key binding string for moving the robot backward.
        /// </summary>
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
        /// <summary>
        /// Gets the key gesture for moving the robot backward.
        /// </summary>
        public KeyGesture MoveBackwardKey => KeyGesture.Parse(MoveBackwardStr);

        private string recordPositionStr = "Z";
        /// <summary>
        /// Gets or sets the key binding string for recording the current position.
        /// </summary>
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
        /// <summary>
        /// Gets the key gesture for recording the current position.
        /// </summary>
        public KeyGesture RecordPositionKey => KeyGesture.Parse(RecordPositionStr);

        private string solveMapStr = "M";
        /// <summary>
        /// Gets or sets the key binding string for solving/calculating the calibration map.
        /// </summary>
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
        /// <summary>
        /// Gets the key gesture for solving the calibration map.
        /// </summary>
        public KeyGesture SolveMapKey => KeyGesture.Parse(SolveMapStr);

        private string increaseStepStr = "+";
        /// <summary>
        /// Gets or sets the key binding string for increasing the movement step size.
        /// </summary>
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
        /// <summary>
        /// Gets the key gesture for increasing the step size.
        /// </summary>
        public KeyGesture IncreaseStepKey => KeyGesture.Parse(IncreaseStepStr);

        private string decreaseStepStr = "-";
        /// <summary>
        /// Gets or sets the key binding string for decreasing the movement step size.
        /// </summary>
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
        /// <summary>
        /// Gets the key gesture for decreasing the step size.
        /// </summary>
        public KeyGesture DecreaseStepKey => KeyGesture.Parse(DecreaseStepStr);

        private string pump1InStr = "D1";
        /// <summary>
        /// Gets or sets the key binding string for Pump 1 aspirate (in) operation.
        /// </summary>
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
        /// <summary>
        /// Gets the key gesture for Pump 1 aspirate operation.
        /// </summary>
        public KeyGesture Pump1InKey => KeyGesture.Parse(Pump1InStr);

        private string pump1OutStr = "Shift+D1";
        /// <summary>
        /// Gets or sets the key binding string for Pump 1 dispense (out) operation.
        /// </summary>
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
        /// <summary>
        /// Gets the key gesture for Pump 1 dispense operation.
        /// </summary>
        public KeyGesture Pump1OutKey => KeyGesture.Parse(Pump1OutStr);

        private string pump2InStr = "D2";
        /// <summary>
        /// Gets or sets the key binding string for Pump 2 aspirate (in) operation.
        /// </summary>
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
        /// <summary>
        /// Gets the key gesture for Pump 2 aspirate operation.
        /// </summary>
        public KeyGesture Pump2InKey => KeyGesture.Parse(Pump2InStr);

        private string pump2OutStr = "Shift+D2";
        /// <summary>
        /// Gets or sets the key binding string for Pump 2 dispense (out) operation.
        /// </summary>
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
        /// <summary>
        /// Gets the key gesture for Pump 2 dispense operation.
        /// </summary>
        public KeyGesture Pump2OutKey => KeyGesture.Parse(Pump2OutStr);

        private string pump3InStr = "D3";
        /// <summary>
        /// Gets or sets the key binding string for Pump 3 aspirate (in) operation.
        /// </summary>
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
        /// <summary>
        /// Gets the key gesture for Pump 3 aspirate operation.
        /// </summary>
        public KeyGesture Pump3InKey => KeyGesture.Parse(Pump3InStr);

        private string pump3OutStr = "Shift+D3";
        /// <summary>
        /// Gets or sets the key binding string for Pump 3 dispense (out) operation.
        /// </summary>
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
        /// <summary>
        /// Gets the key gesture for Pump 3 dispense operation.
        /// </summary>
        public KeyGesture Pump3OutKey => KeyGesture.Parse(Pump3OutStr);

        private string pump4InStr = "D4";
        /// <summary>
        /// Gets or sets the key binding string for Pump 4 aspirate (in) operation.
        /// </summary>
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
        /// <summary>
        /// Gets the key gesture for Pump 4 aspirate operation.
        /// </summary>
        public KeyGesture Pump4InKey => KeyGesture.Parse(Pump4InStr);

        private string pump4OutStr = "Shift+D4";
        /// <summary>
        /// Gets or sets the key binding string for Pump 4 dispense (out) operation.
        /// </summary>
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
        /// <summary>
        /// Gets the key gesture for Pump 4 dispense operation.
        /// </summary>
        public KeyGesture Pump4OutKey => KeyGesture.Parse(Pump4OutStr);

        /// <summary>
        /// Initializes a new instance of the AppKeyBindings class with default key bindings.
        /// </summary>
        public AppKeyBindings() { }

        /// <summary>
        /// Initializes a new instance of the AppKeyBindings class by copying from another instance.
        /// </summary>
        /// <param name="other">The AppKeyBindings instance to copy from.</param>
        public AppKeyBindings(AppKeyBindings other)
        {
            Override(other);
        }

        /// <summary>
        /// Copies all key binding settings from another AppKeyBindings instance.
        /// </summary>
        /// <param name="other">The AppKeyBindings instance to copy from.</param>
        public void Override(AppKeyBindings other)
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
