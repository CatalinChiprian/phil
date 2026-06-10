using CommunityToolkit.Mvvm.Input;
using PHIL_GUI.Models;
using PHIL_GUI.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace PHIL_GUI.ViewModels
{
    /// <summary>
    /// ViewModel for the Pumps window, managing manual pump control for aspirating and dispensing liquids.
    /// Provides controls for selecting pumps, setting volumes, and executing pump operations.
    /// </summary>
    public class PumpsWindowViewModel : ViewModelBase
    {
        /// <summary>
        /// Gets the command to aspirate (draw in) liquid using the selected pump.
        /// </summary>
        public ICommand AspirateCommand { get; }

        /// <summary>
        /// Gets the command to dispense (expel) liquid using the selected pump.
        /// </summary>
        public ICommand DispenseCommand { get; }

        /// <summary>
        /// Gets the command to prime the selected pump by dispensing the maximum volume.
        /// </summary>
        public ICommand PrimeCommand { get; }

        /// <summary>
        /// Gets the command to stop all robot movement.
        /// </summary>
        public ICommand StopCommand { get; }

        /// <summary>
        /// Gets the available pump options for selection (excludes None).
        /// </summary>
        public IEnumerable<Pump> PumpOptions { get; }

        /// <summary>
        /// Gets or sets the currently selected pump.
        /// </summary>
        public Pump SelectedPump { get; set; }

        private int volume;
        /// <summary>
        /// Gets or sets the volume in microliters for aspirate/dispense operations.
        /// </summary>
        public int Volume
        {
            get => volume;
            set => SetProperty(ref volume, value);
        }

        /// <summary>
        /// Gets or sets a function that determines whether a text input control currently has focus.
        /// Used to prevent command execution while user is typing.
        /// </summary>
        public Func<bool> IsTextInputFocused { get; set; } = () => false;

        private bool CanExecutePump() => !IsTextInputFocused();

        /// <summary>
        /// Gets the application's key binding configuration for keyboard shortcuts.
        /// </summary>
        public AppKeyBindings AppKeyBindings => AppSettingsService.AppSettings.AppKeyBindings;

        /// <summary>
        /// Initializes a new instance of the PumpsWindowViewModel class.
        /// Sets default volume, pump options, and configures commands.
        /// </summary>
        public PumpsWindowViewModel()
        {
            Volume = 20;
            PumpOptions = Enum.GetValues<Pump>().Cast<Pump>().SkipLast(1);
            SelectedPump = PumpOptions.First();

            AspirateCommand = new RelayCommand(Aspirate, CanExecutePump);
            DispenseCommand = new RelayCommand(Dispense, CanExecutePump);
            PrimeCommand = new RelayCommand(Prime);
            StopCommand = new RelayCommand(RobotProtocolService.Stop);
        }

        /// <summary>
        /// Aspirates (draws in) the specified volume using the selected pump.
        /// </summary>
        private void Aspirate()
        {
            int pump = (int)SelectedPump;
            RobotProtocolService.Aspirate(pump, Volume);
        }

        /// <summary>
        /// Dispenses (expels) the specified volume using the selected pump.
        /// </summary>
        private void Dispense()
        {
            int pump = (int)SelectedPump;
            RobotProtocolService.Dispense(pump, Volume);
        }

        /// <summary>
        /// Primes the selected pump by dispensing the maximum volume.
        /// </summary>
        private void Prime()
        {
            int pump = (int)SelectedPump;
            RobotProtocolService.Prime(pump);
        }

    }
}
