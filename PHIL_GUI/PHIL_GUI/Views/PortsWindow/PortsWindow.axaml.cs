using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using PHIL_GUI.ViewModels;
using PHIL_GUI.Views;

namespace PHIL_GUI;

public partial class PortsWindow : Window
{
    public PortsWindow()
    {
        InitializeComponent();

        PortsWindowViewModel portsViewModel = new PortsWindowViewModel();
        portsViewModel.Connected += OnConnected;
        Closed += (s,e) => portsViewModel.Connected -= OnConnected;

        DataContext = portsViewModel;
    }

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