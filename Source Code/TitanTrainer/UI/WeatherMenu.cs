using GTA;
using LemonUI.Menus;
using TitanTrainer.Utils;
using TitanTrainer.World;

namespace TitanTrainer.UI
{
    public sealed class WeatherMenu : BaseMenu
    {
        private readonly WeatherManager _weather;

        public WeatherMenu(ServiceContainer services)
            : base("Titan Trainer", "Weather")
        {
            _weather = services.Resolve<WeatherManager>();

            AddWeather("Extra Sunny", Weather.ExtraSunny);
            AddWeather("Clear", Weather.Clear);
            AddWeather("Clouds", Weather.Clouds);
            AddWeather("Smog", Weather.Smog);
            AddWeather("Foggy", Weather.Foggy);
            AddWeather("Overcast", Weather.Overcast);
            AddWeather("Rain", Weather.Raining);
            AddWeather("Thunder", Weather.ThunderStorm);
            AddWeather("Clearing", Weather.Clearing);
            AddWeather("Neutral", Weather.Neutral);
            AddWeather("Snow", Weather.Snowing);
            AddWeather("Blizzard", Weather.Blizzard);
            AddWeather("Light Snow", Weather.Snowlight);
            AddWeather("Christmas", Weather.Christmas);
            AddWeather("Halloween", Weather.Halloween);

            Menu.Add(new NativeSeparatorItem("Options"));

            var freeze = new NativeCheckboxItem("Freeze Weather", false);

            freeze.CheckboxChanged += (sender, args) =>
            {
                _weather.FreezeWeather(freeze.Checked);
            };

            Menu.Add(freeze);
        }

        private void AddWeather(string name, Weather weather)
        {
            var item = new NativeItem(name);

            item.Activated += (s, e) =>
            {
                _weather?.SetWeather(weather);
            };

            Menu.Add(item);
        }
    }
}