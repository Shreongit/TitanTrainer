// Core/Config.cs
using System;
using System.IO;
using Newtonsoft.Json;
using TitanTrainer.Core;

namespace TitanTrainer.Core
{
    /// <summary>
    /// Configuration container for Titan Trainer.
    /// Loads and saves Data/config.json and provides runtime defaults when file is missing or corrupted.
    /// </summary>
    public sealed class Config
    {
        private string _path;

        /// <summary>
        /// Hotkey to toggle the main menu (default F5).
        /// </summary>
        public string ToggleMenuKey { get; set; } = "F5";

        /// <summary>
        /// Whether to enable notifications through the trainer.
        /// </summary>
        public bool EnableNotifications { get; set; } = true;

        /// <summary>
        /// Default vehicle spawn options (example).
        /// </summary>
        public SpawnDefaults SpawnDefaults { get; set; } = new SpawnDefaults();

        /// <summary>
        /// Public parameterless constructor required for JSON deserialization.
        /// Path is set by LoadOrCreate after deserialization.
        /// </summary>
        public Config()
        {
            // default constructor for deserialization
        }

        /// <summary>
        /// Internal constructor that sets the file path.
        /// </summary>
        /// <param name="path">Path to the config file.</param>
        private Config(string path)
        {
            _path = path;
        }

        /// <summary>
        /// Loads configuration from the given path or creates defaults if not found or corrupted.
        /// </summary>
        /// <param name="path">Path to config.json</param>
        /// <returns>Loaded or default Config instance.</returns>
        public static Config LoadOrCreate(string path)
        {
            try
            {
                var full = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
                if (!File.Exists(full))
                {
                    var cfg = new Config(full);
                    cfg.Save();
                    Logger.Info($"Created default config at {full}");
                    return cfg;
                }

                var json = File.ReadAllText(full);
                var cfgFromFile = JsonConvert.DeserializeObject<Config>(json);
                if (cfgFromFile == null)
                {
                    Logger.Warn("Config file was null after deserialization; using defaults.");
                    var cfg = new Config(full);
                    cfg.Save();
                    return cfg;
                }

                // ensure internal path is set to where we loaded from
                cfgFromFile._path = full;
                if (string.IsNullOrWhiteSpace(cfgFromFile.ToggleMenuKey))
                {
                    cfgFromFile.ToggleMenuKey = "F5";
                }
                if (cfgFromFile.SpawnDefaults == null)
                {
                    cfgFromFile.SpawnDefaults = new SpawnDefaults();
                }
                return cfgFromFile;
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to load config: " + ex);
                // If anything goes wrong, return a default config that points to the requested file path
                var fallback = new Config(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path));
                fallback.Save();
                return fallback;
            }
        }

        /// <summary>
        /// Saves the config to disk at the configured path. Safe against IO exceptions.
        /// </summary>
        public void Save()
        {
            try
            {
                var targetPath = _path ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "config.json");
                var directory = Path.GetDirectoryName(targetPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(targetPath, json);
                Logger.Info($"Config saved to {targetPath}");
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to save config: " + ex);
            }
        }
    }

    /// <summary>
    /// Spawn-related default options stored in config.
    /// </summary>
    public class SpawnDefaults
    {
        /// <summary>
        /// Whether to spawn the player inside the vehicle by default.
        /// </summary>
        public bool SpawnInside { get; set; } = true;

        /// <summary>
        /// Whether to apply maximum upgrades on spawn by default.
        /// </summary>
        public bool MaxUpgrade { get; set; } = false;

        // Extend with more defaults that map to spawn options
    }
}