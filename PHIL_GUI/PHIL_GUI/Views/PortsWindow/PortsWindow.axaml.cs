using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using PHIL_GUI.ViewModels;
using PHIL_GUI.Views;

namespace PHIL_GUI;

/// <summary>
/// Window for selecting and connecting to a serial port.
/// </summary>
public partial class PortsWindow : Window
{
    /// <summary>
    /// Initializes the ports window and subscribes to connection events.
    /// </summary>
    public PortsWindow()
    {
        InitializeComponent();

        PortsWindowViewModel portsViewModel = new PortsWindowViewModel();
        portsViewModel.Connected += OnConnected;
        Closed += (s,e) => portsViewModel.Connected -= OnConnected;

        DataContext = portsViewModel;
    }

    /// <summary>
    /// Transitions to the main window after successful connection.
    /// </summary>
    private void OnConnected()
    {
        MainWindow mainWindow = new MainWindow();

#if DEBUG
        mainWindow.AttachDevTools();
#endif

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = mainWindow;
        }

        mainWindow.Show();

        Close();
    }
}