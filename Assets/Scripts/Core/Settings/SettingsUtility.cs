using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace ElevelLabs.VRAvatar.Core.Settings
{
    /// <summary>
    /// Utility class for handling application settings serialization and deserialization.
    /// Provides methods for loading and saving settings from different sources.
    /// </summary>
    public static class SettingsUtility
    {
        /// <summary>
        /// Loads settings from a JSON file.
        /// </summary>
        /// <typeparam name="T">The type of settings to load.</typeparam>
        /// <param name="filePath">The path to the JSON file.</param>
        /// <param name="createIfNotExists">Whether to create a default settings file if one doesn't exist.</param>
        /// <returns>The loaded settings, or default settings if the file doesn't exist.</returns>
        public static T LoadFromFile<T>(string filePath, bool createIfNotExists = true) where T : new()
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    return JsonConvert.DeserializeObject<T>(json);
                }
                else if (createIfNotExists)
                {
                    T defaultSettings = new T();
                    SaveToFile(filePath, defaultSettings);
                    return defaultSettings;
                }
                else
                {
                    Debug.LogWarning($"Settings file not found at {filePath} and createIfNotExists is false.");
                    return new T();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading settings from {filePath}: {ex.Message}");
                return new T();
            }
        }

        /// <summary>
        /// Saves settings to a JSON file.
        /// </summary>
        /// <typeparam name="T">The type of settings to save.</typeparam>
        /// <param name="filePath">The path to the JSON file.</param>
        /// <param name="settings">The settings to save.</param>
        /// <returns>True if successful, false otherwise.</returns>
        public static bool SaveToFile<T>(string filePath, T settings)
        {
            try
            {
                // Ensure directory exists
                string directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(filePath, json);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error saving settings to {filePath}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Loads settings from PlayerPrefs.
        /// </summary>
        /// <typeparam name="T">The type of settings to load.</typeparam>
        /// <param name="key">The PlayerPrefs key to use.</param>
        /// <returns>The loaded settings, or default settings if not found.</returns>
        public static T LoadFromPlayerPrefs<T>(string key) where T : new()
        {
            try
            {
                if (PlayerPrefs.HasKey(key))
                {
                    string json = PlayerPrefs.GetString(key);
                    return JsonConvert.DeserializeObject<T>(json);
                }
                else
                {
                    return new T();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading settings from PlayerPrefs with key {key}: {ex.Message}");
                return new T();
            }
        }

        /// <summary>
        /// Saves settings to PlayerPrefs.
        /// </summary>
        /// <typeparam name="T">The type of settings to save.</typeparam>
        /// <param name="key">The PlayerPrefs key to use.</param>
        /// <param name="settings">The settings to save.</param>
        /// <returns>True if successful, false otherwise.</returns>
        public static bool SaveToPlayerPrefs<T>(string key, T settings)
        {
            try
            {
                string json = JsonConvert.SerializeObject(settings);
                PlayerPrefs.SetString(key, json);
                PlayerPrefs.Save();
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error saving settings to PlayerPrefs with key {key}: {ex.Message}");
                return false;
            }
        }
    }
}