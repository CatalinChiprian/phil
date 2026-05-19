using Avalonia.Controls;
using Avalonia.VisualTree;
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
}