
/* Created by Victoria Shvets
Based on Phillip Dettinger work availible on https://github.com/CSDGroup/PHIL.git */

using System;
using System.Collections.Generic;
using System.Windows.Input;
using PHIL_GUI.Commands;
using PHIL_GUI.Models;
using PHIL_GUI.ViewModels.Base;

namespace PHIL_GUI.ViewModels;

public class MainWindowViewModel : CommunicationBase
{
    public Action Disconnected;
    public ICommand DisconnectCommand { get; }
    public ICommand MoveUpCommand { get; }
    public ICommand MoveDownCommand { get; }

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

    public MainWindowViewModel()
    {
        ConnectedPort = SerialService.PortName;

        _pages = new List<PageItem>
        {
            new PageItem { Title = "Wells", ViewModel = new WellsViewModel() },
            new PageItem { Title = "Calibration", ViewModel = new BasicControlsViewModel() }, // Change VM
            new PageItem { Title = "Medium Exchange", ViewModel = new BasicControlsViewModel() } // Change VM
        };

        SelectedPage = _pages[0];

        DisconnectCommand = new RelayCommand(Disconnect);
        MoveUpCommand = new RelayCommand(() => Send("u"));
        MoveDownCommand = new RelayCommand(() => Send("d"));
    }

    public void Disconnect()
    {
        SerialService.Disconnect();
        Disconnected?.Invoke();
    }
}