using Avalonia.Controls;
using Avalonia.Platform.Storage;
using PHIL_GUI.ViewModels;
using System.IO;

namespace PHIL_GUI.Views;

/// <summary>
/// View for the Debug settings page (communication logs and recording settings).
/// </summary>
public partial class DebugView : UserControl
{
    /// <summary>
    /// Initializes the debug view.
    /// </summary>
    public DebugView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Opens a file save dialog and saves the serial communication log to a text file.
    /// </summary>
    private async void SaveLogButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        TopLevel topLevel = TopLevel.GetTopLevel(this);

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Log File",
            DefaultExtension = "txt",
            SuggestedFileName = "log.txt",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("Text Files")
                {
                    Patterns = new[] { "*.txt" }
                }
            },
        });

        if (file is not null)
        {
            await using var stream = await file.OpenWriteAsync();
            using var streamWriter = new StreamWriter(stream);


            if (DataContext is not DebugViewModel vm) return;


            await streamWriter.WriteLineAsync(vm.RobotProtocolService.ReceivedData);
        }
    }

    /// <summary>
    /// Copies the serial communication log to the clipboard.
    /// </summary>
    public async void CopyButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not DebugViewModel vm) return;

        if (string.IsNullOrEmpty(vm.RobotProtocolService.ReceivedData)) return;

        TopLevel topLevel = TopLevel.GetTopLevel(this);

        if (topLevel is not null)
        {
            await topLevel.Clipboard!.SetTextAsync(vm.RobotProtocolService.ReceivedData);
        }
    }
}