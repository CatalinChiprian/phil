using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using PHIL_GUI.ViewModels;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using System;
using PHIL_GUI.Services;

namespace PHIL_GUI;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; }
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        ServiceCollection services = new ServiceCollection();
        services.AddSingleton<SerialPortService>();
        services.AddSingleton<RobotProtocolService>();
        services.AddTransient<PortsViewModel>();
        services.AddTransient<MainWindowViewModel>();
        Services = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();

            PortsView portsView = new PortsView();

            desktop.MainWindow = portsView;
            portsView.Show();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}