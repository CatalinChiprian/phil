using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PHIL_GUI.Models;
using PHIL_GUI.ViewModels;

namespace PHIL_GUI.Views;

public partial class ActionWindow : Window
{
    public ActionWindow(ActionWindowMode mode, ScheduledAction action)
    {
        InitializeComponent();
        DataContext = new ActionWindowViewModel(mode, action);
    }

    private void BottomBarButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close();
    }
}