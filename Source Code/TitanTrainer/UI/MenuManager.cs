using System;
using LemonUI;
using LemonUI.Menus;
using TitanTrainer.Core;
using TitanTrainer.Utils;

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
            
           

            // Register menus
            _pool.Add(_mainMenu.Menu);
            foreach (var vehicleMenu in _vehicleMenu.AllMenus)
            {
                _pool.Add(vehicleMenu);
            }
            _pool.Add(_settingsMenu.Menu);
            _pool.Add(_weatherMenu.Menu);
            _pool.Add(_timeMenu.Menu);

            _pool.Add(_playerMenu.Menu);
            
            _mainMenu.Hide();
            _vehicleMenu.Hide();
            _settingsMenu.Hide();
            _weatherMenu.Hide();
            _timeMenu.Hide();
            _playerMenu.Hide();

            BuildMainMenu();
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

            //------------------------------------------------
            // Vehicle Spawner
            //------------------------------------------------

            var vehicle = new NativeItem(
                "Vehicle Spawner",
                "Spawn vehicles"
            );

            vehicle.Activated += (s, e) =>
            {
                HideAllMenus();

                _vehicleMenu.Show();
            };

            _mainMenu.Menu.Add(vehicle);
            
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
                        "Titan Trainer\nVersion 0.1"
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
        }

        //------------------------------------------------

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