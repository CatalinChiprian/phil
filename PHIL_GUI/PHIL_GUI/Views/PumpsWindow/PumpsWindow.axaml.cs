using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using PHIL_GUI.ViewModels;
using System;
using System.ComponentModel;

namespace PHIL_GUI.Views;

public partial class PumpsWindow : Window
{
    public PumpsWindow()
    {
        InitializeComponent();
        PumpsWindowViewModel pumpsVm = new PumpsWindowViewModel();
        DataContext = pumpsVm;

        pumpsVm.AppKeyBindings.PropertyChanged += AppKeyBindings_PropertyChanged;
    }

    private void AppKeyBindings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        RefreshKeyBindings();
    }

    private void BottomBarButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close();
    }

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