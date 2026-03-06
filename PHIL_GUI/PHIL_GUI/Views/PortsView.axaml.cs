using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using PHIL_GUI.ViewModels;
using PHIL_GUI.Views;

namespace PHIL_GUI;

public partial class PortsView : Window
{
    public PortsView()
    {
        InitializeComponent();

        PortsViewModel portsViewModel = App.Services.GetRequiredService<PortsViewModel>();
        portsViewModel.Connected += OnConnected;
        Closed += (s,e) => portsViewModel.Connected -= OnConnected;

        DataContext = portsViewModel;
    }

    private void OnConnected()
    {
        MainWindow mainWindow = new MainWindow();
        mainWindow.Show();

        Close();
    }
}