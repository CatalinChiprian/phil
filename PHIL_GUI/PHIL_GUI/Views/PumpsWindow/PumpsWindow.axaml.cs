using Avalonia.Controls;
using Avalonia.Input;
using PHIL_GUI.ViewModels;
using System.ComponentModel;

namespace PHIL_GUI.Views;

/// <summary>
/// Window for manual pump control (aspirate, dispense, prime).
/// </summary>
public partial class PumpsWindow : Window
{
    /// <summary>
    /// Initializes the pumps window and configures keyboard shortcuts.
    /// </summary>
    public PumpsWindow()
    {
        InitializeComponent();
        PumpsWindowViewModel pumpsVm = new PumpsWindowViewModel();

        pumpsVm.IsTextInputFocused = () =>
        {
            var focused = this.FocusManager?.GetFocusedElement();
            return focused is TextBox or NumericUpDown;
        };

        pumpsVm.AppKeyBindings.PropertyChanged += AppKeyBindings_PropertyChanged;

        DataContext = pumpsVm;
    }

    /// <summary>
    /// Refreshes key bindings when app settings change.
    /// </summary>
    private void AppKeyBindings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        RefreshKeyBindings();
    }

    /// <summary>
    /// Closes the pumps window.
    /// </summary>
    private void BottomBarButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close();
    }

    /// <summary>
    /// Rebuilds all key bindings for pump controls from the current settings.
    /// </summary>
    private void RefreshKeyBindings()
    {
        if (DataContext is not PumpsWindowViewModel vm) return;

        KeyBindings.Clear();

        KeyBindings.Add(new KeyBinding
        {
            Gesture = vm.AppKeyBindings.Pump1InKey,
            Command = vm.AspirateCommand,
            CommandParameter = 1,
        });

        KeyBindings.Add(new KeyBinding
        {
            Gesture = vm.AppKeyBindings.Pump1OutKey,
            Command = vm.DispenseCommand,
            CommandParameter = 1,
        });

        KeyBindings.Add(new KeyBinding
        {
            Gesture = vm.AppKeyBindings.Pump2InKey,
            Command = vm.AspirateCommand,
            CommandParameter = 2,
        });

        KeyBindings.Add(new KeyBinding
        {
            Gesture = vm.AppKeyBindings.Pump2OutKey,
            Command = vm.DispenseCommand,
            CommandParameter = 2,
        });

        KeyBindings.Add(new KeyBinding
        {
            Gesture = vm.AppKeyBindings.Pump3InKey,
            Command = vm.AspirateCommand,
            CommandParameter = 3,
        });

        KeyBindings.Add(new KeyBinding
        {
            Gesture = vm.AppKeyBindings.Pump3OutKey,
            Command = vm.DispenseCommand,
            CommandParameter = 3,
        });

        KeyBindings.Add(new KeyBinding
        {
            Gesture = vm.AppKeyBindings.Pump4InKey,
            Command = vm.AspirateCommand,
            CommandParameter = 4,
        });

        KeyBindings.Add(new KeyBinding
        {
            Gesture = vm.AppKeyBindings.Pump4OutKey,
            Command = vm.DispenseCommand,
            CommandParameter = 4,
        });
    }
}