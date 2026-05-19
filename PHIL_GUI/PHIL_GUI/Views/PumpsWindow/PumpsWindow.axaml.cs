using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PHIL_GUI.ViewModels;

namespace PHIL_GUI.Views;

public partial class PumpsWindow : Window
{
    public PumpsWindow()
    {
        InitializeComponent();
        DataContext = new PumpsWindowViewModel();
    }

    private void BottomBarButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close();
    }
}