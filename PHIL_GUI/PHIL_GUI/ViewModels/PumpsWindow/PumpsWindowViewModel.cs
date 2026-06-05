using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using PHIL_GUI.Models;
using PHIL_GUI.ViewModels.Base;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace PHIL_GUI.ViewModels
{
    public class PumpsWindowViewModel : ViewModelBase
    {
        public ICommand AspirateCommand { get; }
        public ICommand DispenseCommand { get; }
        public ICommand PrimeCommand { get; }
        public ICommand StopCommand { get; }

        public IEnumerable<Pump> PumpOptions { get; }
        public Pump SelectedPump { get; set; }

        private int volume;
        public int Volume
        {
            get => volume;
            set => SetProperty(ref volume, value);
        }

        public Func<bool> IsTextInputFocused { get; set; } = () => false;

        private bool CanExecutePump() => !IsTextInputFocused();

        public AppKeyBindings AppKeyBindings => AppSettingsService.AppSettings.AppKeyBindings;

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

        private void Aspirate()
        {
            int pump = (int)SelectedPump;
            RobotProtocolService.Aspirate(pump, Volume);
        }

        private void Dispense()
        {
            int pump = (int)SelectedPump;
            RobotProtocolService.Dispense(pump, Volume);
        }

        private void Prime()
        {
            int pump = (int)SelectedPump;
            RobotProtocolService.Prime(pump);
        }

    }
}
