
/* Created by Victoria Shvets
Based on Phillip Dettinger work availible on https://github.com/CSDGroup/PHIL.git */

using PHIL_GUI.Commands;
using PHIL_GUI.Models;
using PHIL_GUI.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace PHIL_GUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public Action Disconnected;
        public ICommand DisconnectCommand { get; }
        public ICommand OpenSettingsCommand { get; }
        public ICommand MoveUpCommand { get; }
        public ICommand MoveDownCommand { get; }
        public ICommand GoHomeCommand { get; }
        public ICommand CalibrateHomeCommand { get; }
        public ICommand SelectWell96Command { get; }
        public ICommand SelectOrganOnChipCommand { get; }
        public ICommand EmergencyStopCommand { get; }
        public List<PageItem> Pages { get; }
        private PageItem selectedPage;
        public PageItem SelectedPage
        {
            get => selectedPage;
            set
            {
                SetProperty(ref selectedPage, value);

                CurrentPage = value.ViewModel;
            }
        }

        public string ConnectedPort { get; }

        private object currentPage;
        public object CurrentPage
        {
            get => currentPage;
            set => SetProperty(ref currentPage, value);
        }

        public Well CurrentWell => RobotProtocol.RobotState.CurrentWell;
        public LimitSwitches Limit => RobotProtocol.RobotState.Limit;
        public Settings Settings => RobotProtocol.RobotState.Settings;

        public MainWindowViewModel()
        {
            ConnectedPort = RobotProtocol.SerialPort.PortName;

            Pages = new List<PageItem>
            {
                new PageItem("Wells", new WellsViewModel()),
                new PageItem("Calibration", new CalibrationViewModel()),
                new PageItem("Medium Exchange", new BasicControlsViewModel()) // Change VM
            };

            SelectedPage = Pages[0];

            DisconnectCommand = new RelayCommand(Disconnect);
            OpenSettingsCommand = new RelayCommand(OpenSettings);
            MoveUpCommand = new RelayCommand(RobotProtocol.MoveUp);
            MoveDownCommand = new RelayCommand(RobotProtocol.MoveDown);
            GoHomeCommand = new RelayCommand(RobotProtocol.GoHome);
            CalibrateHomeCommand = new RelayCommand(RobotProtocol.CalibrateHome);
            SelectWell96Command = new RelayCommand(() => Settings.SelectedPlateType = PlateType.Well96);
            SelectOrganOnChipCommand = new RelayCommand(() => Settings.SelectedPlateType = PlateType.OrganOnChip);
            EmergencyStopCommand = new RelayCommand(RobotProtocol.EmergencyStop);
        }

        private void Disconnect()
        {
            RobotProtocol.SerialPort.Disconnect();
            Disconnected?.Invoke();
        }

        private void OpenSettings()
        {
            new SettingsWindow().Show();
        }
    }
}