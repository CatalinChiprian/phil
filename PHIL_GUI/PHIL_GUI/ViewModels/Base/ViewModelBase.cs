using Microsoft.Extensions.DependencyInjection;
using PHIL_GUI.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PHIL_GUI.ViewModels.Base;

public class ViewModelBase : INotifyPropertyChanged
{
    protected readonly RobotProtocolService robotProtocol;
    public RobotProtocolService RobotProtocol => robotProtocol;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected ViewModelBase()
    {
        robotProtocol = App.Services.GetRequiredService<RobotProtocolService>();
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}