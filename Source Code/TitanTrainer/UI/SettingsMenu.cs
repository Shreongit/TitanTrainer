using System;
using LemonUI.Menus;
using TitanTrainer.Core;
using TitanTrainer.Utils;

namespace TitanTrainer.UI
{
    /// <summary>
    /// Titan Trainer settings menu.
    /// </summary>
    public class SettingsMenu
    {
        private readonly ServiceContainer _services;
        private readonly Config _config;

        public NativeMenu Menu { get; }

        public SettingsMenu(ServiceContainer services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));

            _config = _services.Resolve<Config>()
                ?? throw new InvalidOperationException("Config service not found.");

            Menu = new NativeMenu("Settings", "Trainer Settings");
            Menu.Visible = false;

            BuildMenu();
        }

        private void BuildMenu()
        {
            Menu.Clear();
            
            var back = new NativeItem("<< Back", "Return to Main Menu");

            back.Activated += (s, e) =>
            {
                Hide();
            };

            Menu.Add(back);

            var notifications = new NativeItem(
                $"Notifications : {(_config.EnableNotifications ? "ON" : "OFF")}",
                "Toggle notifications"
            );

            notifications.Activated += (s, e) =>
            {
                _config.EnableNotifications = !_config.EnableNotifications;
                _config.Save();

                BuildMenu();

                _services.Resolve<NotificationService>()
                    ?.NotifyInfo($"Notifications {(_config.EnableNotifications ? "Enabled" : "Disabled")}");
            };

            Menu.Add(notifications);

            var spawnInside = new NativeItem(
                $"Spawn Inside Vehicle : {(_config.SpawnDefaults.SpawnInside ? "YES" : "NO")}",
                "Toggle spawn inside vehicle"
            );

            spawnInside.Activated += (s, e) =>
            {
                _config.SpawnDefaults.SpawnInside =
                    !_config.SpawnDefaults.SpawnInside;

                _config.Save();

                BuildMenu();

                _services.Resolve<NotificationService>()
                    ?.NotifyInfo($"Spawn Inside = {_config.SpawnDefaults.SpawnInside}");
            };

            Menu.Add(spawnInside);

            var maxUpgrade = new NativeItem(
                $"Max Upgrade : {(_config.SpawnDefaults.MaxUpgrade ? "YES" : "NO")}",
                "Toggle max upgrades"
            );

            maxUpgrade.Activated += (s, e) =>
            {
                _config.SpawnDefaults.MaxUpgrade =
                    !_config.SpawnDefaults.MaxUpgrade;

                _config.Save();

                BuildMenu();

                _services.Resolve<NotificationService>()
                    ?.NotifyInfo($"Max Upgrade = {_config.SpawnDefaults.MaxUpgrade}");
            };

            Menu.Add(maxUpgrade);
        }

        public void Show()
        {
            Menu.Visible = true;
        }

        public void Hide()
        {
            Menu.Visible = false;
        }
    }
}