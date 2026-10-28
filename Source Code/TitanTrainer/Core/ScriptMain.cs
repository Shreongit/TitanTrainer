using System;
using System.Windows.Forms;
using GTA;
using TitanTrainer.Core;
using TitanTrainer.UI;
using TitanTrainer.Utils;
using TitanTrainer.Vehicles;
using TitanTrainer.World;
using TitanTrainer.Player;

namespace TitanTrainer
{
    /// <summary>
    /// The main script entry point for Titan Trainer.
    /// Boots services, initializes managers, and handles game ticks.
    /// </summary>
    public class ScriptMain : Script
    {
        private readonly ServiceContainer _services;
        private Config _config;
        private Keys _toggleMenuKey = Keys.F5;
        private bool _initialized;
        private bool _disposed;

        private readonly NotificationService _notificationService;

        private readonly VehicleDatabase _vehicleDatabase;
        private readonly VehicleManager _vehicleManager;

        private readonly WeatherManager _weatherManager;
        private readonly TimeManager _timeManager;
        
        private readonly PlayerManager _playerManager;
        private readonly EntitySpawner _entitySpawner;
        private readonly SceneBuilderService _sceneBuilderService;

        private readonly MenuManager _menuManager;

        public ScriptMain()
        {
            try
            {
                Logger.Initialize("TitanTrainer");
                Logger.Info("Initializing Titan Trainer...");

                _services = new ServiceContainer();

                // Config
                _config = Config.LoadOrCreate("Data/config.json");
                _services.Register(_config);

                if (!Enum.TryParse(_config.ToggleMenuKey, true, out _toggleMenuKey) || _toggleMenuKey == Keys.None)
                {
                    _toggleMenuKey = Keys.F5;
                    _config.ToggleMenuKey = Keys.F5.ToString();
                    _config.Save();
                    Logger.Warn("Invalid ToggleMenuKey in config; using F5.");
                }

                // Notification Service
                _notificationService = new NotificationService();
                _services.Register(_notificationService);

                // Vehicle System
                _vehicleDatabase = new VehicleDatabase("Data/vehicles.json");
                _services.Register(_vehicleDatabase);

                _vehicleManager = new VehicleManager(_vehicleDatabase, _notificationService);
                _services.Register(_vehicleManager);

                // World System
                _weatherManager = new WeatherManager(_notificationService);
                _services.Register(_weatherManager);

                _timeManager = new TimeManager(_notificationService);
                _services.Register(_timeManager);
                
                // Player System
                _playerManager = new PlayerManager(_notificationService);
                _services.Register(_playerManager);

                // Entity spawning (props and pedestrians)
                _entitySpawner = new EntitySpawner(_notificationService);
                _services.Register(_entitySpawner);

                _sceneBuilderService = new SceneBuilderService(_entitySpawner, _notificationService, "Data/scenes.json");
                _services.Register(_sceneBuilderService);

                // UI
                _menuManager = new MenuManager(_services);
                _services.Register(_menuManager);

                Tick += OnTick;
                KeyDown += OnKeyDown;

                Interval = 0;

                Logger.Info("Titan Trainer initialized successfully.");

                _initialized = true;
                if (_config.EnableNotifications)
                {
                    _notificationService.NotifySuccess("Titan Trainer Loaded");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Initialization failed: " + ex);

                try
                {
                    _notificationService?.NotifyError("Failed to initialize Titan Trainer.");
                }
                catch
                {
                }
            }
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!_initialized || _disposed) return;

            try
            {
                _weatherManager?.Update();
                
                _timeManager?.Update();

                
                _playerManager?.Update();

                _vehicleManager?.Update();


                _menuManager?.Process();
            }
            catch (Exception ex)
            {
                Logger.Error("Tick Error: " + ex);
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (!_initialized || _disposed) return;

            try
            {
                if (e.KeyCode == Keys.Back && _menuManager.HandleBackspace())
                {
                    // Prevent this keypress from being treated as a generic close action.
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    return;
                }

                if (e.KeyCode == _toggleMenuKey)
                {
                    _menuManager.ToggleMainMenu();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("KeyDown Error: " + ex);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _initialized = false;

            try
            {
                Tick -= OnTick;
                KeyDown -= OnKeyDown;

                _menuManager?.Dispose();

                _weatherManager?.Dispose();
                _timeManager?.Dispose();
                _playerManager?.Dispose();
                _entitySpawner?.Dispose();

                _vehicleManager?.Dispose();
                _vehicleDatabase?.Dispose();

                Logger.Info("Titan Trainer disposed successfully.");
            }
            catch (Exception ex)
            {
                Logger.Error("Dispose Error: " + ex);
            }
        }
    }
}