using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using System;
using PHIL_GUI.Services;
using PHIL_GUI.ViewModels;
using PHIL_GUI.Models;

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

        services.AddSingleton<RobotProtocolService>();
        services.AddSingleton<AppSettingsService>();

        services.AddSingleton<IPlateContext>(sp =>
            sp.GetRequiredService<AppSettingsService>().AppSettings
        );
        services.AddSingleton<IRecordContext>(sp =>
            sp.GetRequiredService<AppSettingsService>().AppSettings
        );

        Services = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();

            PortsWindow portsWindow = new PortsWindow();

            desktop.MainWindow = portsWindow;
            portsWindow.Show();


#if DEBUG
            this.AttachDevTools();
#endif
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