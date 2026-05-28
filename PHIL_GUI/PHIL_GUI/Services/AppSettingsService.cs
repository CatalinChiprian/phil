using CommunityToolkit.Mvvm.ComponentModel;
using PHIL_GUI.Models;
using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace PHIL_GUI.Services
{
    public class AppSettingsService : ObservableObject
    {
        private static readonly string SettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PHIL", "settings.json");
        public AppSettings AppSettings { get; } = new AppSettings();

        public AppSettingsService()
        {
            Load();
        }

        public void Load()
        {
            try
            {
                if (!File.Exists(SettingsPath)) throw new Exception();
                var json = File.ReadAllText(SettingsPath);
                var loaded = JsonSerializer.Deserialize<AppSettings>(json) ?? throw new Exception();

                ApplySettings(loaded);

            }
            catch 
            { 
            }
        }

        public void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath));
            File.WriteAllText(SettingsPath, JsonSerializer.Serialize(AppSettings, new JsonSerializerOptions { WriteIndented = true }));
        }


        private void ApplySettings(AppSettings loaded)
        {
            AppSettings.SelectedPlateType = loaded.SelectedPlateType;
            AppSettings.AppKeyBindings.Override(loaded.AppKeyBindings);
        }
    }
}
