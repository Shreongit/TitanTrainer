using System;
using LemonUI;
using LemonUI.Menus;
using TitanTrainer.Core;
using TitanTrainer.Utils;
using TitanTrainer.World;

namespace TitanTrainer.UI
{
    /// <summary>
    /// Handles all trainer menus.
    /// </summary>
    public sealed class MenuManager : IDisposable
    {
        private readonly ObjectPool _pool;

        private readonly MainMenu _mainMenu;
        private readonly VehicleMenu _vehicleMenu;
        private readonly SettingsMenu _settingsMenu;
        private readonly WeatherMenu _weatherMenu;
        private readonly PlayerMenu _playerMenu;
        private readonly TimeMenu _timeMenu;
        private readonly WeaponsMenu _weaponsMenu;
        private readonly TeleportMenu _teleportMenu;
        private readonly EntitySpawnerMenu _entitySpawnerMenu;
        private readonly SceneBuilderMenu _sceneBuilderMenu;
        
        private readonly ServiceContainer _services;

        public MenuManager(ServiceContainer services)
        {
            _services = services;

            _pool = new ObjectPool();

            // Create menus
            _mainMenu = new MainMenu();
            _vehicleMenu = new VehicleMenu(services);
            _settingsMenu = new SettingsMenu(services);
            _weatherMenu = new WeatherMenu(services);
            _timeMenu = new TimeMenu(services);
            _playerMenu = new PlayerMenu(services);
            _weaponsMenu = new WeaponsMenu(services.Resolve<NotificationService>(), () => { HideAllMenus(); _mainMenu.Show(); });
            var playerManager = services.Resolve<TitanTrainer.Player.PlayerManager>();
            _teleportMenu = new TeleportMenu(playerManager.Teleport, services.Resolve<NotificationService>(), () => { HideAllMenus(); _mainMenu.Show(); });
            _entitySpawnerMenu = new EntitySpawnerMenu(services.Resolve<EntitySpawner>(), () => { HideAllMenus(); _mainMenu.Show(); });
            _sceneBuilderMenu = new SceneBuilderMenu(services.Resolve<SceneBuilderService>(), () => { HideAllMenus(); _mainMenu.Show(); });
            
           

            // Register menus
            _pool.Add(_mainMenu.Menu);
            foreach (var vehicleMenu in _vehicleMenu.AllMenus)
            {
                _pool.Add(vehicleMenu);
            }
            foreach (var menu in _settingsMenu.AllMenus) _pool.Add(menu);
            foreach (var menu in _weatherMenu.AllMenus) _pool.Add(menu);
            foreach (var menu in _timeMenu.AllMenus) _pool.Add(menu);
            foreach (var menu in _playerMenu.AllMenus) _pool.Add(menu);
            foreach (var menu in _weaponsMenu.AllMenus) _pool.Add(menu);
            foreach (var menu in _teleportMenu.AllMenus) _pool.Add(menu);
            foreach (var entityMenu in _entitySpawnerMenu.AllMenus) _pool.Add(entityMenu);
            foreach (var sceneMenu in _sceneBuilderMenu.AllMenus) _pool.Add(sceneMenu);
            
            _mainMenu.Hide();
            _vehicleMenu.Hide();
            _settingsMenu.Hide();
            _weatherMenu.Hide();
            _timeMenu.Hide();
            _playerMenu.Hide();
            _weaponsMenu.Hide();
            _teleportMenu.Hide();
            _entitySpawnerMenu.Hide();
            _sceneBuilderMenu.Hide();

            BuildMainMenu();
            MenuNavigation.RegisterParent(_playerMenu.Menu, _mainMenu.Menu);
            MenuNavigation.RegisterParent(_weaponsMenu.Menu, _mainMenu.Menu);
            MenuNavigation.RegisterParent(_teleportMenu.Menu, _mainMenu.Menu);
            MenuNavigation.RegisterParent(_entitySpawnerMenu.Menu, _mainMenu.Menu);
            MenuNavigation.RegisterParent(_sceneBuilderMenu.Menu, _mainMenu.Menu);
            MenuNavigation.RegisterParent(_weatherMenu.Menu, _mainMenu.Menu);
            MenuNavigation.RegisterParent(_timeMenu.Menu, _mainMenu.Menu);
            MenuNavigation.RegisterParent(_settingsMenu.Menu, _mainMenu.Menu);
        }

        private void BuildMainMenu()
        {
            _mainMenu.Menu.Clear();
            
            //------------------------------------------------
// Player
//------------------------------------------------

            var player = new NativeItem(
                "Player",
                "Player options"
            );

            player.Activated += (s, e) =>
            {
                HideAllMenus();

                _playerMenu.Show();
            };

            _mainMenu.Menu.Add(player);

            var weapons = new NativeItem("Weapons", "Weapons, ammunition and loadouts");
            weapons.Activated += (s, e) => { HideAllMenus(); _weaponsMenu.Show(); };
            _mainMenu.Menu.Add(weapons);

            var teleport = new NativeItem("Teleport", "Waypoint and saved locations");
            teleport.Activated += (s, e) => { HideAllMenus(); _teleportMenu.Show(); };
            _mainMenu.Menu.Add(teleport);

            //------------------------------------------------
            // Vehicles
            //------------------------------------------------
            var vehicles = new NativeItem("Vehicles", "Vehicle controls, customization, and spawner");
            vehicles.Activated += (s, e) => { HideAllMenus(); _vehicleMenu.Show(); };
            _mainMenu.Menu.Add(vehicles);

            var entities = new NativeItem("Entity Spawner", "Spawn props and pedestrians");
            entities.Activated += (s, e) => { HideAllMenus(); _entitySpawnerMenu.Show(); };
            _mainMenu.Menu.Add(entities);

            var sceneBuilder = new NativeItem("Scene Builder", "Save and load arrangements of props, pedestrians, and vehicles");
            sceneBuilder.Activated += (s, e) => { HideAllMenus(); _sceneBuilderMenu.Show(); };
            _mainMenu.Menu.Add(sceneBuilder);

            //------------------------------------------------
// Weather
//------------------------------------------------

            var weather = new NativeItem(
                "Weather",
                "Control GTA V weather"
            );

            weather.Activated += (s, e) =>
            {
                HideAllMenus();

                _weatherMenu.Show();
            };

            _mainMenu.Menu.Add(weather);
            
            
            //------------------------------------------------
// Time
//------------------------------------------------

            var time = new NativeItem(
                "Time",
                "Control GTA V time"
            );

            time.Activated += (s, e) =>
            {
                HideAllMenus();

                _timeMenu.Show();
            };

            _mainMenu.Menu.Add(time);

            //------------------------------------------------
            // Settings
            //------------------------------------------------

            var settings = new NativeItem(
                "Settings",
                "Trainer settings"
            );

            settings.Activated += (s, e) =>
            {
                HideAllMenus();

                _settingsMenu.Show();
            };

            _mainMenu.Menu.Add(settings);

            //------------------------------------------------
            // About
            //------------------------------------------------

            var about = new NativeItem(
                "About",
                "Titan Trainer information"
            );

            about.Activated += (s, e) =>
            {
                _services.Resolve<NotificationService>()
                    ?.NotifyInfo(
                        "Titan Trainer\nVersion 0.5.0"
                    );
            };

            _mainMenu.Menu.Add(about);
        }

        //------------------------------------------------

        private void HideAllMenus()
        {
            _mainMenu.Hide();
            _playerMenu.Hide();
            _vehicleMenu.Hide();
            _weatherMenu.Hide();
            _timeMenu.Hide();
            _settingsMenu.Hide();
            _weaponsMenu.Hide();
            _teleportMenu.Hide();
            _entitySpawnerMenu.Hide();
            _sceneBuilderMenu.Hide();
        }

        //------------------------------------------------

        /// <summary>
        /// Routes Backspace to the vehicle-menu hierarchy when one is active.
        /// </summary>
        public bool HandleBackspace()
        {
            if (_vehicleMenu.HandleBackspace())
            {
                if (_vehicleMenu.RootWasClosedByBackspace) { HideAllMenus(); _mainMenu.Show(); _vehicleMenu.ClearRootBackspaceFlag(); }
                return true;
            }
            if (MenuNavigation.TryBack(_playerMenu.AllMenus) || MenuNavigation.TryBack(_weaponsMenu.AllMenus) ||
                MenuNavigation.TryBack(_teleportMenu.AllMenus) || MenuNavigation.TryBack(_settingsMenu.AllMenus) ||
                MenuNavigation.TryBack(_weatherMenu.AllMenus) || MenuNavigation.TryBack(_timeMenu.AllMenus) ||
                MenuNavigation.TryBack(_entitySpawnerMenu.AllMenus) || MenuNavigation.TryBack(_sceneBuilderMenu.AllMenus)) return true;
            return false;
        }

        public void ToggleMainMenu()
        {
            if (_mainMenu.Menu.Visible)
            {
                HideAllMenus();
            }
            else
            {
                HideAllMenus();

                _mainMenu.Show();
            }
        }

        //------------------------------------------------

        public void Process()
        {
            _pool.Process();
        }

        //------------------------------------------------

        public void Dispose()
        {
            HideAllMenus();
        }
    }
}