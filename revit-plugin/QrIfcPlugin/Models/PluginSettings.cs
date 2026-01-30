using System;
using System.IO;
using Newtonsoft.Json;

namespace QrIfcPlugin.Models
{
    /// <summary>
    /// Plugin configuration settings
    /// </summary>
    public class PluginSettings
    {
        /// <summary>
        /// Base URL of the backend API (e.g., https://api.example.com)
        /// </summary>
        public string ApiBaseUrl { get; set; } = "http://localhost:5000";

        /// <summary>
        /// Project slug identifier
        /// </summary>
        public string ProjectSlug { get; set; } = "sample-project";

        /// <summary>
        /// Default model version tag
        /// </summary>
        public string DefaultModelVersion { get; set; } = "v1";

        /// <summary>
        /// Default QR code size in millimeters
        /// </summary>
        public double QrSizeMm { get; set; } = 30.0;

        /// <summary>
        /// Whether to show label with element mark
        /// </summary>
        public bool ShowLabel { get; set; } = true;

        /// <summary>
        /// Token expiry in days
        /// </summary>
        public int ExpiryDays { get; set; } = 90;

        /// <summary>
        /// Viewer base URL (e.g., https://viewer.example.com)
        /// </summary>
        public string ViewerBaseUrl { get; set; } = "http://localhost:3000";

        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "QrIfcPlugin",
            "settings.json"
        );

        /// <summary>
        /// Load settings from disk
        /// </summary>
        public static PluginSettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    return JsonConvert.DeserializeObject<PluginSettings>(json) ?? new PluginSettings();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load settings: {ex.Message}");
            }

            return new PluginSettings();
        }

        /// <summary>
        /// Save settings to disk
        /// </summary>
        public void Save()
        {
            try
            {
                var directory = Path.GetDirectoryName(SettingsPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(SettingsPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save settings: {ex.Message}");
                throw;
            }
        }
    }
}
