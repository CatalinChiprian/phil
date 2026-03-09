using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using PHIL_GUI.ViewModels.Base;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using RelayCommand = PHIL_GUI.Commands.RelayCommand;

namespace PHIL_GUI.ViewModels;

public class WellsViewModel : CommunicationBase
{ 
    public ICommand EmergencyStopCommand { get; }
    public ICommand GoHomeCommand { get; }
    public ICommand SelectPlateTypeCommand { get; }
    public ICommand WellsPositionCommand { get; }
    public ICommand MoveUpCommand { get; }
    public ICommand MoveDownCommand { get; }

    public ObservableCollection<string> Wells { get; } = new();
    public int WellsCount { get; set; } = 12;

    public double RmsL = 0.57;
    public double RmsR = 0.8;
    public double CalCount = 40;
    public double CalMax = 40;
    public int Microsteps = 8;

    public string RmsDisplayText => $"L {RmsL:F2}°  R {RmsR:F2}°";
    public IBrush RmsColor
    {
        get
        {
            double worst = Math.Max(RmsL, RmsR);
            if (worst > 1.5) return Application.Current.Resources["Warn"] as IBrush ?? Brushes.Red;
            if (worst > 1.0) return Application.Current.Resources["Caution"] as IBrush ?? Brushes.Orange;
            return Application.Current.Resources["Accent"] as IBrush ?? Brushes.Green;
        }
    }
    public string CalPointsText => $"{CalCount}/{CalMax}";
    public IBrush CalPointsColor
    {
        get
        {
            if (CalCount < 10) return Application.Current.Resources["Warn"] as IBrush ?? Brushes.Red;
            if (CalCount < 20) return Application.Current.Resources["Caution"] as IBrush ?? Brushes.Orange;
            return Application.Current.Resources["Accent"] as IBrush ?? Brushes.Green;
        }
    }
    public string MicrostepsText => $"1/{Microsteps}";
    
    public WellsViewModel()
    {
        EmergencyStopCommand = new RelayCommand(() => Send("s"));
        GoHomeCommand = new RelayCommand(() => Send("h"));
        MoveUpCommand = new RelayCommand(() => Send("u"));
        MoveDownCommand = new RelayCommand(() => Send("d"));
        WellsPositionCommand = new RelayCommand<string>(w => Send($"w{w?.ToLower()}"));

        var rows = new[] { "A", "B", "C", "D", "E", "F", "G", "H" };

        foreach (var row in rows)
        {
            for (int col = 1; col <= WellsCount; col++)
            {
                Wells.Add($"{row}{col}");
            }
        }
    }
}