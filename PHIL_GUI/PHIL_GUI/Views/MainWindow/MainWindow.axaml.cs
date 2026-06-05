using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Microsoft.Extensions.DependencyInjection;
using PHIL_GUI.Models;
using PHIL_GUI.ViewModels;

namespace PHIL_GUI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        this.IsEnabled = false;

        MainWindowViewModel mainVm = new MainWindowViewModel();
        mainVm.AppKeyBindings.PropertyChanged += AppKeyBindings_PropertyChanged;
        mainVm.RobotProtocolService.OnAppInitialized += RobotProtocolService_OnAppInitialized;

        mainVm.Disconnected += OnDisconnected;
        Closed += (s, e) => mainVm.Disconnected -= OnDisconnected;

        DataContext = mainVm;
    }

    private void RobotProtocolService_OnAppInitialized()
    {
        this.IsEnabled = true;
    }

    private void AppKeyBindings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        RefreshKeyBindings();
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

    private void RefreshKeyBindings()
    {
        if (DataContext is not MainWindowViewModel vm) return;

        KeyBindings.Clear();

        KeyBindings.Add(new KeyBinding
        {
            Gesture = vm.AppKeyBindings.GoHomeKey,
            Command = vm.GoHomeCommand
        });

        KeyBindings.Add(new KeyBinding
        {
            Gesture = vm.AppKeyBindings.CalibrateHomeKey,
            Command = vm.CalibrateHomeCommand
        });

        KeyBindings.Add(new KeyBinding
        {
            Gesture = vm.AppKeyBindings.MoveUpKey,
            Command = vm.MoveUpCommand
        });

        KeyBindings.Add(new KeyBinding
        {
            Gesture = vm.AppKeyBindings.MoveDownKey,
            Command = vm.MoveDownCommand
        });
    }
}