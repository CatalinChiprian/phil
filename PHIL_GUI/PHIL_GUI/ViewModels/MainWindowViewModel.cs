
/* Created by Victoria Shvets
Based on Phillip Dettinger work availible on https://github.com/CSDGroup/PHIL.git */

using PHIL_GUI.Commands;
using PHIL_GUI.Models;
using PHIL_GUI.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace PHIL_GUI.ViewModels;

public class MainWindowViewModel : CommunicationBase
{
    public Action Disconnected;
    public ICommand DisconnectCommand { get; }
    public ICommand MoveUpCommand { get; }
    public ICommand MoveDownCommand { get; }
    public ICommand EmergencyStopCommand { get; }
    public ICommand GoHomeCommand { get; }
    public ICommand SelectWell96Command { get; }
    public ICommand SelectOrganOnChipCommand { get; }

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

    public Well CurrentWell => RobotState.CurrentWell;
    public LimitSwitches Limit => RobotState.Limit;
    public Settings Settings => RobotState.Settings;

    public MainWindowViewModel()
    {
        ConnectedPort = SerialService.PortName;

        _pages = new List<PageItem>
        {
            new PageItem { Title = "Wells", ViewModel = new WellsViewModel() },
            new PageItem { Title = "Calibration", ViewModel = new CalibrationViewModel() }, // Change VM
            new PageItem { Title = "Medium Exchange", ViewModel = new BasicControlsViewModel() } // Change VM
        };

        SelectedPage = _pages[0];

        DisconnectCommand = new RelayCommand(Disconnect);
        MoveUpCommand = new RelayCommand(() => Send("u"));
        MoveDownCommand = new RelayCommand(() => Send("d"));
        EmergencyStopCommand = new RelayCommand(EmergencyStop);
        GoHomeCommand = new RelayCommand(GoHome);
        SelectWell96Command = new RelayCommand(() => Settings.SelectedPlateType = PlateType.Well96);
        SelectOrganOnChipCommand = new RelayCommand(() => Settings.SelectedPlateType = PlateType.OrganOnChip);
    }

    public void Disconnect()
    {
        SerialService.Disconnect();
        Disconnected?.Invoke();
    }
}