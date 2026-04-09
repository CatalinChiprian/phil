using Avalonia.Controls;
using Avalonia.Platform.Storage;
using PHIL_GUI.ViewModels;
using System.IO;
using static CommunityToolkit.Mvvm.ComponentModel.__Internals.__TaskExtensions.TaskAwaitableWithoutEndValidation;

namespace PHIL_GUI.Views;

public partial class DebugView : UserControl
{
    public DebugView()
    {
        InitializeComponent();
    }

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


            await streamWriter.WriteLineAsync(vm.ReceivedData);
        }
    }

    public async void CopyButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not DebugViewModel vm) return;

        if (string.IsNullOrEmpty(vm.ReceivedData)) return;

        TopLevel topLevel = TopLevel.GetTopLevel(this);

        if (topLevel is not null)
        {
            await topLevel.Clipboard!.SetTextAsync(vm.ReceivedData);
        }
    }
}