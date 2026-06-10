using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using PHIL_GUI.Services;

namespace PHIL_GUI.ViewModels.Base;

/// <summary>
/// Base class for all ViewModels in the application, providing access to core services.
/// Inherits from ObservableObject to support property change notifications for data binding.
/// </summary>
public class ViewModelBase : ObservableObject
{
    /// <summary>
    /// Application settings service for accessing and modifying user preferences and configuration.
    /// </summary>
    protected readonly AppSettingsService appSettingsService;

    /// <summary>
    /// Gets the application settings service instance.
    /// </summary>
    public AppSettingsService AppSettingsService => appSettingsService;

    /// <summary>
    /// Robot protocol service for communicating with the robot hardware and managing robot state.
    /// </summary>
    protected readonly RobotProtocolService robotProtocolService;

    /// <summary>
    /// Gets the robot protocol service instance.
    /// </summary>
    public RobotProtocolService RobotProtocolService => robotProtocolService;

    /// <summary>
    /// Initializes a new instance of the ViewModelBase class and resolves required services from dependency injection.
    /// </summary>
    protected ViewModelBase()
    {
        appSettingsService = App.Services.GetRequiredService<AppSettingsService>();
        robotProtocolService = App.Services.GetRequiredService<RobotProtocolService>();
    }
}