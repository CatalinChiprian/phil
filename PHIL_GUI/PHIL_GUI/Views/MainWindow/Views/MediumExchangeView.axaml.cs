using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
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
}