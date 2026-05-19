using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using PHIL_GUI.ViewModels;

namespace PHIL_GUI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        MainWindowViewModel mainVm = App.Services.GetRequiredService<MainWindowViewModel>();
        mainVm.Disconnected += OnDisconnected;
        Closed += (s, e) => mainVm.Disconnected -= OnDisconnected;

        DataContext = mainVm;
    }

    private async void OnDisconnected()
    {
        PortsWindow portsWindow = new PortsWindow();
        await portsWindow.ShowDialog(this);

        Close();
    }

    private async void SettingsButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        await new SettingsWindow().ShowDialog(this);
    }
}