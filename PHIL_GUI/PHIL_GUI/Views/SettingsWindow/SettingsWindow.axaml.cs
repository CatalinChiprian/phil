using Avalonia.Controls;
using PHIL_GUI.ViewModels;

namespace PHIL_GUI.Views;

/// <summary>
/// Window for application settings (plate type, controls, debug).
/// </summary>
public partial class SettingsWindow : Window
{
    /// <summary>
    /// Initializes the settings window.
    /// </summary>
    public SettingsWindow()
    {
        InitializeComponent();

        DataContext = new SettingsWindowViewModel();
    }

    /// <summary>
    /// Closes the settings window.
    /// </summary>
    private void BottomBarButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close();
    }
}