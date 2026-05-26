using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Extensions.DependencyInjection;
using PHIL_GUI.ViewModels;

namespace PHIL_GUI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        MainWindowViewModel mainVm = new MainWindowViewModel();
        mainVm.Disconnected += OnDisconnected;
        Closed += (s, e) => mainVm.Disconnected -= OnDisconnected;

        DataContext = mainVm;
    }


    private void OnDisconnected()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop) return;

        var portsWindow = new PortsWindow();

#if DEBUG
        portsWindow.AttachDevTools();
#endif

        desktop.MainWindow = portsWindow;

        portsWindow.Show();
        Close();
    }

    private async void SettingsButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        await new SettingsWindow().ShowDialog(this);
    }
}