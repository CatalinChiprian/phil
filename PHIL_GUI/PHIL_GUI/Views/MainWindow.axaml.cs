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

        mainVm.GoToWellsView();
        DataContext = mainVm;
    }

    private void OnDisconnected()
    {
        PortsView portsView = new PortsView();
        portsView.Show();

        Close();
    }
}