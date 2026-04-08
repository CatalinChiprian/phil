using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using PHIL_GUI.ViewModels;

namespace PHIL_GUI;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();

        SettingsWindowViewModel settingsViewModel = App.Services.GetRequiredService<SettingsWindowViewModel>();

        DataContext = settingsViewModel;
    }
}