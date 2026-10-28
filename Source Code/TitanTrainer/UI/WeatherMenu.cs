using System.Collections.Generic;
using System.Linq;
using LemonUI.Menus;
using GTA;
using TitanTrainer.Utils;
using TitanTrainer.World;

namespace TitanTrainer.UI
{
    public sealed class WeatherMenu : BaseMenu
    {
        private readonly WeatherManager _weather;
        private readonly List<NativeMenu> _allMenus = new List<NativeMenu>();
        public NativeMenu[] AllMenus => _allMenus.ToArray();
        public WeatherMenu(ServiceContainer services) : base("Titan Trainer", "Weather")
        {
            _weather = services.Resolve<WeatherManager>(); _allMenus.Add(Menu);
            var clear = MenuNavigation.CreateChild(Menu, "Clear & Cloudy", "Sunny, clear, cloud, and smog presets", _allMenus);
            var rain = MenuNavigation.CreateChild(Menu, "Rain & Storms", "Rain, thunder, fog, and overcast presets", _allMenus);
            var snow = MenuNavigation.CreateChild(Menu, "Snow & Special Weather", "Snow, blizzard, holiday, and special presets", _allMenus);
            var options = MenuNavigation.CreateChild(Menu, "Weather Options", "Freeze or unfreeze the current weather", _allMenus);
            foreach (var child in new[] { clear, rain, snow, options }) MenuNavigation.AddBack(child, Menu, _allMenus);
            AddWeather(clear, "Extra Sunny", Weather.ExtraSunny); AddWeather(clear, "Clear", Weather.Clear); AddWeather(clear, "Clouds", Weather.Clouds); AddWeather(clear, "Smog", Weather.Smog); AddWeather(clear, "Neutral", Weather.Neutral);
            AddWeather(rain, "Foggy", Weather.Foggy); AddWeather(rain, "Overcast", Weather.Overcast); AddWeather(rain, "Rain", Weather.Raining); AddWeather(rain, "Thunder", Weather.ThunderStorm); AddWeather(rain, "Clearing", Weather.Clearing);
            AddWeather(snow, "Snow", Weather.Snowing); AddWeather(snow, "Blizzard", Weather.Blizzard); AddWeather(snow, "Light Snow", Weather.Snowlight); AddWeather(snow, "Christmas", Weather.Christmas); AddWeather(snow, "Halloween", Weather.Halloween);
            var freeze = new NativeCheckboxItem("Freeze Weather", false); freeze.CheckboxChanged += (s,e) => _weather.FreezeWeather(freeze.Checked); options.Add(freeze);
        }
        private void AddWeather(NativeMenu target, string name, Weather weather) { var item = new NativeItem(name, "Apply this weather preset"); item.Activated += (s,e) => _weather?.SetWeather(weather); target.Add(item); }
        public override void Show() { MenuNavigation.Hide(_allMenus); Menu.Visible = true; }
        public override void Hide() => MenuNavigation.Hide(_allMenus);
    }
}
