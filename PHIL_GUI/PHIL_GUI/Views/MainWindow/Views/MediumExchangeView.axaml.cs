using Avalonia.Controls;
using Avalonia.VisualTree;
using PHIL_GUI.Models;
using PHIL_GUI.ViewModels;

namespace PHIL_GUI.Views;

/// <summary>
/// View for the Medium Exchange page showing scheduled actions and well selection.
/// </summary>
public partial class MediumExchangeView : UserControl
{
    /// <summary>
    /// Initializes the medium exchange view.
    /// </summary>
    public MediumExchangeView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Handles pointer press on well-pair borders to select organ-on-chip wells.
    /// </summary>
    private void Border_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (DataContext is not MediumExchangeViewModel vm) return;

        if (sender is not Border border) return;

        if (border.Tag is not int target) return;

        vm.SelectTargetCommand.Execute(target.ToString());
    }

    /// <summary>
    /// Opens the action window in create mode.
    /// </summary>
    private async void CreateActionButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var parent = this.GetVisualRoot() as Window;
        if (parent == null) return;

        await new ActionWindow(ActionWindowMode.Create, null).ShowDialog(parent);
    }

    /// <summary>
    /// Opens the action window in edit mode for the selected action.
    /// </summary>
    private async void EditActionButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var parent = this.GetVisualRoot() as Window;
        if (parent == null) return;

        if (DataContext is not MediumExchangeViewModel vm) return;

        if (sender is not Button button) return;
        if (button.Tag is not ActionItem actionItem) return;

        await new ActionWindow(ActionWindowMode.Update, actionItem).ShowDialog(parent);
    }
    /// <summary>
    /// Deletes the selected action from the scheduler.
    /// </summary>
    private void DeleteActionButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not MediumExchangeViewModel vm) return;

        if (sender is not Button button) return;
        if (button.Tag is not int actionId) return;

        vm.DeleteAction(actionId);
    }
    /// <summary>
    /// Attaches an available action to the selected well(s).
    /// </summary>
    private void AttachActionButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not MediumExchangeViewModel vm) return;

        if (sender is not Button button) return;
        if (button.Tag is not ActionItem action) return;

        vm.AttachAction(action);
    }
    /// <summary>
    /// Detaches an action from the selected well(s).
    /// </summary>
    private void DetachActionButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not MediumExchangeViewModel vm) return;

        if (sender is not Button button) return;
        if (button.Tag is not ActionItem action) return;

        vm.DetachAction(action);
    }
}