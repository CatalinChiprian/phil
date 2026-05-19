using Avalonia.Controls;
using Avalonia.VisualTree;

namespace PHIL_GUI.Views;

public partial class CalibrationView : UserControl
{
    public CalibrationView()
    {
        InitializeComponent();
    }

    private async void PumpsButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var parent = this.GetVisualRoot() as Window;
        if (parent == null) return;

        await new PumpsWindow().ShowDialog(parent);
    }
}