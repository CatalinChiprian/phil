using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using PHIL_GUI.Models;
using PHIL_GUI.Services;
using System;
using System.Linq;

namespace PHIL_GUI;

/// <summary>
/// Main application class managing dependency injection and application lifecycle.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Gets the dependency injection service provider for the application.
    /// </summary>
    public static IServiceProvider Services { get; private set; }
    /// <summary>
    /// Loads XAML resources during application initialization.
    /// </summary>
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    /// Configures dependency injection and initializes the application lifecycle.
    /// </summary>
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

            Views.PortsWindow portsWindow = new Views.PortsWindow();

            desktop.MainWindow = portsWindow;
            portsWindow.Show();


#if DEBUG
            this.AttachDevTools();
#endif
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// Disables Avalonia's built-in data annotation validation to avoid conflicts with CommunityToolkit.
    /// </summary>
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