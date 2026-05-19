using PHIL_GUI.Commands;
using PHIL_GUI.Models;
using PHIL_GUI.ViewModels.Base;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace PHIL_GUI.ViewModels
{
    public class PumpsWindowViewModel : ViewModelBase
    {
        public ICommand AspirateCommand { get; }
        public ICommand DispenseCommand { get; }
        public ICommand PrimeCommand { get; }
        public ICommand StopCommand { get; }
        public ObservableCollection<string> PumpOptions { get; set; }
        public string SelectedPump { get; set; }

        private int volume;
        public int Volume
        {
            get => volume;
            set => SetProperty(ref volume, value);
        }

        public AppKeyBindings AppKeyBindings => AppSettingsService.AppSettings.AppKeyBindings;

        public PumpsWindowViewModel()
        {
            Volume = 20;
            PumpOptions = new ObservableCollection<string> { "P1", "P2", "P3", "P4" };
            SelectedPump = PumpOptions[0];

            AspirateCommand = new RelayCommand(Aspirate);
            DispenseCommand = new RelayCommand(Dispense);
            PrimeCommand = new RelayCommand(Prime);
            StopCommand = new RelayCommand(RobotProtocolService.Stop);
        }

        private void Aspirate()
        {
            int pump = int.Parse(SelectedPump.Substring(1));
            
            RobotProtocolService.Aspirate(pump, Volume);
        }

        private void Dispense()
        {
            int pump = int.Parse(SelectedPump.Substring(1));
            RobotProtocolService.Dispense(pump, Volume);
        }

        private void Prime()
        {
            int pump = int.Parse(SelectedPump.Substring(1));
            RobotProtocolService.Prime(pump);
        }

    }
}
