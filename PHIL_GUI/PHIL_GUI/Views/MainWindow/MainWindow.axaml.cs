using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using PHIL_GUI.ViewModels;

namespace PHIL_GUI.Views;

/// <summary>
/// Main application window view.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// Initializes the main window and sets up event handlers for ViewModel events.
    /// </summary>
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

    /// <summary>
    /// Enables the main window after the robot protocol service initializes.
    /// </summary>
    private void RobotProtocolService_OnAppInitialized()
    {
        this.IsEnabled = true;
    }

    /// <summary>
    /// Refreshes key bindings when app settings change.
    /// </summary>
    private void AppKeyBindings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        RefreshKeyBindings();
    }

    /// <summary>
    /// Handles disconnection by returning to the ports selection window.
    /// </summary>
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

    /// <summary>
    /// Opens the settings window as a modal dialog.
    /// </summary>
    private async void SettingsButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        await new SettingsWindow().ShowDialog(this);
    }

    /// <summary>
    /// Rebuilds all key bindings from the current ViewModel settings.
    /// </summary>
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