using Newtonsoft.Json;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;

namespace ElevenLabsTTS.Models
{
    public class Configuration
    {
        public string ApiKey { get; set; } = string.Empty;
        public string SelectedVoiceId { get; set; } = string.Empty;
        public string SelectedVoiceName { get; set; } = string.Empty;
        public string SelectedModel { get; set; } = "eleven_multilingual_v2";
        public int Stability { get; set; } = 50;
        public int SimilarityBoost { get; set; } = 75;
        public int Style { get; set; } = 0;
        public bool UseSpeakerBoost { get; set; } = true;
        public int Speed { get; set; } = 50;
        public string OutputFormat { get; set; } = "mp3_44100_192";
        public int LastFileNumber { get; set; } = 0;
        public string Language { get; set; } = "English"; // Default language is English

        // Dictionary of supported languages and their codes for ElevenLabs API
        public static readonly Dictionary<string, string> SupportedLanguages = new Dictionary<string, string>
        {
            { "English", "en" },
            { "French", "fr" },
            { "Spanish", "es" },
            { "Italian", "it" },
            { "German", "de" },
            { "Dutch", "nl" },
            { "Chinese", "zh" }
        };

        private static readonly string ConfigFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ElevenLabsTTS",
            "config.json"
        );

        public static Configuration Load()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ConfigFilePath));
                if (File.Exists(ConfigFilePath))
                {
                    var json = File.ReadAllText(ConfigFilePath);
                    var config = JsonConvert.DeserializeObject<Configuration>(json) ?? new Configuration();
                    return config;
                }
                return new Configuration();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading configuration: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new Configuration();
            }
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ConfigFilePath));
                var json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(ConfigFilePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving configuration: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public string GetFileExtension()
        {
            return OutputFormat.StartsWith("mp3") ? "mp3" : "wav";
        }
    }
} 