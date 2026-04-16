using PHIL_GUI.Models;
using System;
using System.IO;
using System.Text.Json;

namespace PHIL_GUI.Services
{
    public class AppSettingsService
    {
        private static readonly string SettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PHIL", "settings.json");
        public AppSettings AppSettings { get; set; }

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
                AppSettings = JsonSerializer.Deserialize<AppSettings>(json) ?? throw new Exception();
            }
            catch 
            { 
                AppSettings = new AppSettings();
            }
        }

        public void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath));
            File.WriteAllText(SettingsPath, JsonSerializer.Serialize(AppSettings, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
