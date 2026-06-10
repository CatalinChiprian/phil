using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PHIL_GUI.Models;
using PHIL_GUI.ViewModels;

namespace PHIL_GUI.Views;

/// <summary>
/// Window for creating and editing scheduled actions.
/// </summary>
public partial class ActionWindow : Window
{
    /// <summary>
    /// Initializes the action window with a specific mode and optional existing action.
    /// </summary>
    /// <param name="mode">Creation or update mode.</param>
    /// <param name="action">Existing action to edit, or null for new action.</param>
    public ActionWindow(ActionWindowMode mode, ActionItem action)
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


    /// <summary>
    /// Initializes the action window in default mode (used by designer).
    /// </summary>
    public ActionWindow()
    {
        InitializeComponent();
        DataContext = new ActionWindowViewModel();
    }


    /// <summary>
    /// Handles save and cancel button clicks.
    /// </summary>
    private void BottomBarButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not ActionWindowViewModel vm) return;
        if (sender is not Button button) return;

        if (button.Tag?.ToString() == "Save")
        {
            vm.Save();
            if (vm.DisplayError) return;
        }

        Close();
    }
}