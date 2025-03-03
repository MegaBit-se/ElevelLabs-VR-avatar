using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace ElevelLabs.VRAvatar.Core
{
    /// <summary>
    /// Handles loading, saving, and accessing application configuration settings.
    /// </summary>
    public class ConfigManager : MonoBehaviour
    {
        private static ConfigManager _instance;
        public static ConfigManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    Debug.LogError("ConfigManager is not initialized. Make sure it exists in the scene.");
                }
                return _instance;
            }
        }

        [SerializeField] private string configFileName = "AppSettings.json";
        private AppSettings _appSettings;

        public AppSettings Settings => _appSettings;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            LoadSettings();
        }

        /// <summary>
        /// Loads application settings from the JSON configuration file.
        /// </summary>
        public void LoadSettings()
        {
            try
            {
                string configPath = Path.Combine(Application.streamingAssetsPath, "Settings", configFileName);
                
                if (File.Exists(configPath))
                {
                    string json = File.ReadAllText(configPath);
                    _appSettings = JsonConvert.DeserializeObject<AppSettings>(json);
                    Debug.Log("Settings loaded successfully");
                }
                else
                {
                    Debug.LogWarning($"Settings file not found at {configPath}. Creating default settings.");
                    _appSettings = new AppSettings();
                    SaveSettings(); // Create default settings file
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading settings: {ex.Message}");
                _appSettings = new AppSettings(); // Use defaults if loading fails
            }
        }

        /// <summary>
        /// Saves the current application settings to the JSON configuration file.
        /// </summary>
        public void SaveSettings()
        {
            try
            {
                string configDirectory = Path.Combine(Application.streamingAssetsPath, "Settings");
                string configPath = Path.Combine(configDirectory, configFileName);

                // Ensure directory exists
                if (!Directory.Exists(configDirectory))
                {
                    Directory.CreateDirectory(configDirectory);
                }

                string json = JsonConvert.SerializeObject(_appSettings, Formatting.Indented);
                File.WriteAllText(configPath, json);
                Debug.Log("Settings saved successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error saving settings: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the API key in settings and saves the configuration.
        /// </summary>
        public void UpdateApiKey(string apiKey)
        {
            _appSettings.ElevenLabsApiKey = apiKey;
            SaveSettings();
        }
    }

    /// <summary>
    /// Data structure for application settings.
    /// </summary>
    [Serializable]
    public class AppSettings
    {
        // ElevenLabs API Settings
        public string ElevenLabsApiKey = "";
        public string VoiceId = "default";
        public float SpeechRate = 1.0f;
        public float Stability = 0.5f;
        public float Similarity = 0.75f;
        
        // Audio Settings
        public float MicrophoneSensitivity = 1.0f;
        public float ResponseVolume = 1.0f;
        public bool AutoDetectSpeech = true;
        public float SilenceThreshold = 0.05f;
        
        // Avatar Settings
        public string DefaultAvatarModel = "default";
        public float LipSyncSensitivity = 1.0f;
        public bool EnableIdleAnimations = true;
        
        // UI Settings
        public bool ShowSubtitles = true;
        public int MaxConversationHistory = 10;
        public string UiTheme = "default";
    }
}