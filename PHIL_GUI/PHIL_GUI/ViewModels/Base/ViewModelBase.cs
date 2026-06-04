using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using PHIL_GUI.Services;
using System.ComponentModel;

namespace PHIL_GUI.ViewModels.Base;

public class ViewModelBase : ObservableObject
{
    protected readonly AppSettingsService appSettingsService;
    public AppSettingsService AppSettingsService => appSettingsService;

    protected readonly RobotProtocolService robotProtocolService;
    public RobotProtocolService RobotProtocolService => robotProtocolService;
    protected ViewModelBase()
    {
        appSettingsService = App.Services.GetRequiredService<AppSettingsService>();
        robotProtocolService = App.Services.GetRequiredService<RobotProtocolService>();
    }
}