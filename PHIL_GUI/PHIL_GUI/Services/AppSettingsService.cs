using CommunityToolkit.Mvvm.ComponentModel;
using PHIL_GUI.Models;
using System;
using System.IO;
using System.Text.Json;

namespace PHIL_GUI.Services
{
    /// <summary>
    /// Service for loading, saving, and managing application settings.
    /// Settings are persisted to a JSON file in the user's AppData folder.
    /// </summary>
    public class AppSettingsService : ObservableObject
    {
        private static readonly string SettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PHIL", "settings.json");
        /// <summary>
        /// Gets the current application settings.
        /// </summary>
        public AppSettings AppSettings { get; } = new AppSettings();

        /// <summary>
        /// Initializes a new instance of the AppSettingsService class and loads saved settings.
        /// </summary>
        public AppSettingsService()
        {
            Load();
        }

        /// <summary>
        /// Loads settings from the JSON file. If the file doesn't exist or is invalid, uses default settings.
        /// </summary>
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

        /// <summary>
        /// Saves the current settings to the JSON file in AppData.
        /// Creates the directory if it doesn't exist.
        /// </summary>
        public void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath));
            File.WriteAllText(SettingsPath, JsonSerializer.Serialize(AppSettings, new JsonSerializerOptions { WriteIndented = true }));
        }


        /// <summary>
        /// Applies loaded settings to the current AppSettings instance.
        /// </summary>
        /// <param name="loaded">The settings loaded from file.</param>
        private void ApplySettings(AppSettings loaded)
        {
            AppSettings.SelectedPlateType = loaded.SelectedPlateType;
            AppSettings.AreActionRecorded = loaded.AreActionRecorded;
            AppSettings.AppKeyBindings.Override(loaded.AppKeyBindings);
        }
    }
}
