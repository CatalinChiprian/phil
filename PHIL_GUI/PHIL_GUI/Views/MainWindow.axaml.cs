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

    private void OnDisconnected()
    {
        PortsWindow portsWindow = new PortsWindow();
        portsWindow.Show();

        Close();
    }
}