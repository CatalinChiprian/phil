using Avalonia.Controls;
using Avalonia.VisualTree;

namespace PHIL_GUI.Views;

/// <summary>
/// View for the Calibration page showing manual robot control and calibration point recording.
/// </summary>
public partial class CalibrationView : UserControl
{
    /// <summary>
    /// Initializes the calibration view.
    /// </summary>
    public CalibrationView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Opens the pumps control window as a modal dialog.
    /// </summary>
    private async void PumpsButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var parent = this.GetVisualRoot() as Window;
        if (parent == null) return;

        await new PumpsWindow().ShowDialog(parent);
    }
}