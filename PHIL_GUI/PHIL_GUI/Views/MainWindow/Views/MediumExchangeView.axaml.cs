using Avalonia.Controls;
using Avalonia.VisualTree;
using PHIL_GUI.Models;
using PHIL_GUI.ViewModels;

namespace PHIL_GUI.Views;

public partial class MediumExchangeView : UserControl
{
    public MediumExchangeView()
    {
        InitializeComponent();
    }

    private void Border_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (DataContext is not MediumExchangeViewModel vm) return;

        if (sender is not Border border) return;

        if (border.Tag is not int target) return;

        vm.SelectTargetCommand.Execute(target.ToString());
    }

    private async void CreateActionButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var parent = this.GetVisualRoot() as Window;
        if (parent == null) return;

        await new ActionWindow(ActionWindowMode.Create, null).ShowDialog(parent);
    }

    private async void EditActionButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var parent = this.GetVisualRoot() as Window;
        if (parent == null) return;

        if (DataContext is not MediumExchangeViewModel vm) return;

        if (sender is not Button button) return;
        if (button.Tag is not ActionItem actionItem) return;

        await new ActionWindow(ActionWindowMode.Update, actionItem).ShowDialog(parent);
    }
    private void DeleteActionButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not MediumExchangeViewModel vm) return;

        if (sender is not Button button) return;
        if (button.Tag is not int actionId) return;

        vm.DeleteAction(actionId);
    }
    private void AttachActionButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not MediumExchangeViewModel vm) return;

        if (sender is not Button button) return;
        if (button.Tag is not ActionItem action) return;

        vm.AttachAction(action);
    }
    private void DetachActionButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not MediumExchangeViewModel vm) return;

        if (sender is not Button button) return;
        if (button.Tag is not ActionItem action) return;

        vm.DetachAction(action);
    }
}