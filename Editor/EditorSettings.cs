using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Editor
{
    public enum MouseButton
    {
        Left,
        Right,
        Middle
    }

    public enum ZoomModifier
    {
        None,
        Ctrl,
        Shift
    }

    public enum AppTheme
    {
        Dark,
        Light,
        System
    }

    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(EditorSettings))]
    internal partial class EditorSettingsContext : JsonSerializerContext { }

    public class EditorSettings
    {
        private static readonly string SettingsDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WalkerSim2", "Editor");

        private static readonly string SettingsPath = Path.Combine(SettingsDir, "settings.json");

        private static EditorSettings _instance;

        public static EditorSettings Instance
        {
            get
            {
                if (_instance == null)
                    _instance = Load();
                return _instance;
            }
        }

        // Appearance
        public AppTheme Theme { get; set; } = AppTheme.Dark;

        // Canvas controls
        public MouseButton PanButton { get; set; } = MouseButton.Right;
        public ZoomModifier ZoomModifier { get; set; } = ZoomModifier.Ctrl;

        // Game folders
        public List<string> GameFolders { get; set; } = new List<string>();

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(SettingsDir);
                var json = JsonSerializer.Serialize(this, EditorSettingsContext.Default.EditorSettings);
                File.WriteAllText(SettingsPath, json);
            }
            catch (Exception ex)
            {
                WalkerSim.Logging.Warn("Failed to save editor settings: {0}", ex.Message);
            }
        }

        public static EditorSettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    var settings = JsonSerializer.Deserialize(json, EditorSettingsContext.Default.EditorSettings);
                    if (settings != null)
                        return settings;
                }
            }
            catch (Exception ex)
            {
                WalkerSim.Logging.Warn("Failed to load editor settings, using defaults: {0}", ex.Message);
            }
            return new EditorSettings();
        }

        public void ResetToDefaults()
        {
            Theme = AppTheme.Dark;
            PanButton = MouseButton.Right;
            ZoomModifier = ZoomModifier.Ctrl;
            GameFolders.Clear();
        }
    }
}
