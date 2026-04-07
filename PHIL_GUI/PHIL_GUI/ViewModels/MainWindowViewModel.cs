
/* Created by Victoria Shvets
Based on Phillip Dettinger work availible on https://github.com/CSDGroup/PHIL.git */

using PHIL_GUI.Commands;
using PHIL_GUI.Models;
using PHIL_GUI.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace PHIL_GUI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public Action Disconnected;
    public ICommand DisconnectCommand { get; }
    public ICommand MoveUpCommand { get; }
    public ICommand MoveDownCommand { get; }
    public ICommand GoHomeCommand { get; }
    public ICommand CalibrateHomeCommand { get; }
    public ICommand SelectWell96Command { get; }
    public ICommand SelectOrganOnChipCommand { get; }
    public ICommand EmergencyStopCommand { get; }

    private object _currentPage;
    private List<PageItem> _pages;
    public List<PageItem> Pages => _pages;
    private PageItem _selectedPage;
    public PageItem SelectedPage
    {
        get => _selectedPage;
        set
        {
            if (SetProperty(ref _selectedPage, value))
                CurrentPage = value.ViewModel;
        }
    }

    public string ConnectedPort { get; }
    public object CurrentPage
    {
        get => _currentPage;
        set => SetProperty(ref _currentPage, value);
    }

    public Well CurrentWell => RobotProtocol.RobotState.CurrentWell;
    public LimitSwitches Limit => RobotProtocol.RobotState.Limit;
    public Settings Settings => RobotProtocol.RobotState.Settings;

    public MainWindowViewModel()
    {
        ConnectedPort = RobotProtocol.SerialPort.PortName;

        _pages = new List<PageItem>
        {
            new PageItem { Title = "Wells", ViewModel = new WellsViewModel() },
            new PageItem { Title = "Calibration", ViewModel = new CalibrationViewModel() },
            new PageItem { Title = "Medium Exchange", ViewModel = new BasicControlsViewModel() } // Change VM
        };

        SelectedPage = _pages[0];

        DisconnectCommand = new RelayCommand(Disconnect);
        MoveUpCommand = new RelayCommand(RobotProtocol.MoveUp);
        MoveDownCommand = new RelayCommand(RobotProtocol.MoveDown);
        GoHomeCommand = new RelayCommand(RobotProtocol.GoHome);
        CalibrateHomeCommand = new RelayCommand(RobotProtocol.CalibrateHome);
        SelectWell96Command = new RelayCommand(() => Settings.SelectedPlateType = PlateType.Well96);
        SelectOrganOnChipCommand = new RelayCommand(() => Settings.SelectedPlateType = PlateType.OrganOnChip);
        EmergencyStopCommand = new RelayCommand(RobotProtocol.EmergencyStop);
    }

    public void Disconnect()
    {
        RobotProtocol.SerialPort.Disconnect();
        Disconnected?.Invoke();
    }
}