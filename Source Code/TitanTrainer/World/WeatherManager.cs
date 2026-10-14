using System;
using GTA;
using TitanTrainer.Core;
using TitanTrainer.Utils;

namespace TitanTrainer.World
{
    /// <summary>
    /// Handles all weather-related operations.
    /// This class contains no UI logic and can safely be reused
    /// by menus, hotkeys, presets and future profile systems.
    /// </summary>
    public sealed class WeatherManager : IDisposable
    {
        private readonly NotificationService _notifications;

        private Weather _currentWeather = Weather.Clear;

        private bool _freezeWeather;

        public WeatherManager(NotificationService notifications)
        {
            _notifications = notifications;
            _currentWeather = GTA.World.Weather;
        }

        /// <summary>
        /// Gets the current weather.
        /// </summary>
        public Weather CurrentWeather => _currentWeather;

        /// <summary>
        /// Returns whether weather is frozen.
        /// </summary>
        public bool IsWeatherFrozen => _freezeWeather;

        /// <summary>
        /// Sets weather instantly.
        /// </summary>
        public void SetWeather(Weather weather)
        {
            try
            {
                _currentWeather = weather;
                GTA.World.Weather = weather;

                Logger.Info($"Weather changed to {weather}");

                _notifications?.NotifySuccess($"Weather: {weather}");
            }
            catch (Exception ex)
            {
                Logger.Error($"WeatherManager.SetWeather() failed: {ex}");
                _notifications?.NotifyError("Unable to change weather.");
            }
        }

        /// <summary>
        /// Smooth transition.
        /// </summary>
        public void TransitionToWeather(Weather weather, int milliseconds = 5000)
        {
            try
            {
                _currentWeather = weather;

                GTA.World.TransitionToWeather(weather, milliseconds);

                Logger.Info($"Transitioning weather to {weather}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Weather transition failed: {ex}");
            }
        }

        /// <summary>
        /// Freeze or unfreeze weather.
        /// </summary>
        public void FreezeWeather(bool freeze)
        {
            _freezeWeather = freeze;

            _notifications?.NotifyInfo(
                freeze
                    ? "Weather Frozen"
                    : "Weather Unfrozen");
        }

        /// <summary>
        /// Called every game tick.
        /// Keeps weather locked if freeze is enabled.
        /// </summary>
        public void Update()
        {
            if (!_freezeWeather)
                return;

            if (GTA.World.Weather != _currentWeather)
            {
                GTA.World.Weather = _currentWeather;
            }
        }

        public void Dispose()
        {
        }
    }
}