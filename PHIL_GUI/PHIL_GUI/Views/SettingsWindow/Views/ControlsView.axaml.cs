using Avalonia.Controls;
using Avalonia.Input;
using PHIL_GUI.Models;
using PHIL_GUI.ViewModels;
using System.Collections.Generic;

namespace PHIL_GUI.Views;

/// <summary>
/// View for the Controls settings page (keyboard shortcut configuration).
/// </summary>
public partial class ControlsView : UserControl
{
    /// <summary>
    /// Previous key binding string before editing.
    /// </summary>
    string? prevStr;
    /// <summary>
    /// Currently active button being configured.
    /// </summary>
    Button? activeButton;
    /// <summary>
    /// Initializes the controls view and subscribes to key events.
    /// </summary>
    public ControlsView()
    {
        InitializeComponent();

        KeyDown += OnKeyDown;
    }

    /// <summary>
    /// Captures keyboard input and assigns it to the active keybinding button.
    /// </summary>
    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (activeButton == null) return;
        if (DataContext is not ControlsViewModel vm) return;

        var parts = new List<string>();

        if (e.KeyModifiers.HasFlag(KeyModifiers.Control)) parts.Add("Ctrl");
        if (e.KeyModifiers.HasFlag(KeyModifiers.Shift)) parts.Add("Shift");
        if (e.KeyModifiers.HasFlag(KeyModifiers.Alt)) parts.Add("Alt");

        if (e.Key is Key.LeftShift or Key.RightShift or
            Key.LeftCtrl or Key.RightCtrl or
            Key.LeftAlt or Key.RightAlt)
            return;

        parts.Add(e.Key.ToString());

        string keyStr = string.Join("+", parts);

        if (e.Key == Key.Escape)
        {
            activeButton.Content = prevStr;
        }
        else
        {
            string? propertyName = activeButton.Tag as string;

            if (propertyName != null)
            {
                typeof(AppKeyBindings)
                    .GetProperty(propertyName)
                    ?.SetValue(vm.AppKeyBindings, keyStr);
            }
        }

        activeButton.Classes.Remove("listening");
        activeButton = null;
    }

    /// <summary>
    /// Activates a keybinding button for editing.
    /// </summary>
    private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (activeButton != null)
        {
            activeButton.Content = prevStr;
            activeButton.Classes.Remove("listening");
        }
        activeButton = sender as Button;

        if (activeButton == null) return;

        prevStr = activeButton.Content as string;

        activeButton.Content = "...";
        activeButton.Classes.Add("listening");
    }
}