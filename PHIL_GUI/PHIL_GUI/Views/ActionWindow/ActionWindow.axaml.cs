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

        Title = mode switch
        {
            ActionWindowMode.Create => "New Action - PHIL",
            ActionWindowMode.Update => "Edit Action - PHIL",
            _ => "Action Menu - PHIL",
        };
    }


    public ActionWindow()
    {
        InitializeComponent();
        DataContext = new ActionWindowViewModel();
    }


    private void BottomBarButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not ActionWindowViewModel vm) return;
        if (sender is not Button button) return;

        if (button.Tag?.ToString() == "Save") vm.Save();
        if (vm.DisplayError) return;

        Close();
    }
}