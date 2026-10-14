using LemonUI.Menus;
using TitanTrainer.Utils;
using TitanTrainer.World;

namespace TitanTrainer.UI
{
    public sealed class TimeMenu : BaseMenu
    {
        private readonly TimeManager _time;

        public TimeMenu(ServiceContainer services)
            : base("Titan Trainer", "Time")
        {
            _time = services.Resolve<TimeManager>();

            AddTime("Midnight (00:00)", 0, 0);
            AddTime("03:00", 3, 0);
            AddTime("Sunrise (06:00)", 6, 0);
            AddTime("09:00", 9, 0);
            AddTime("Noon (12:00)", 12, 0);
            AddTime("15:00", 15, 0);
            AddTime("Sunset (18:00)", 18, 0);
            AddTime("21:00", 21, 0);

            Menu.Add(new NativeSeparatorItem("Adjust Time"));

            var addHour = new NativeItem(
                "+1 Hour",
                "Advance one hour"
            );

            addHour.Activated += (s, e) =>
            {
                _time.AddHour();
            };

            Menu.Add(addHour);

            var removeHour = new NativeItem(
                "-1 Hour",
                "Go back one hour"
            );

            removeHour.Activated += (s, e) =>
            {
                _time.RemoveHour();
            };

            Menu.Add(removeHour);

            var addMinute = new NativeItem(
                "+30 Minutes",
                "Advance thirty minutes"
            );

            addMinute.Activated += (s, e) =>
            {
                _time.AddMinute();
            };

            Menu.Add(addMinute);

            var removeMinute = new NativeItem(
                "-30 Minutes",
                "Go back thirty minutes"
            );

            removeMinute.Activated += (s, e) =>
            {
                _time.RemoveMinute();
            };

            Menu.Add(removeMinute);

            Menu.Add(new NativeSeparatorItem("Options"));

            var freeze = new NativeCheckboxItem(
                "Freeze Time",
                false
            );

            freeze.CheckboxChanged += (s, e) =>
            {
                _time.FreezeTime(freeze.Checked);
            };

            Menu.Add(freeze);
        }

        private void AddTime(string name, int hour, int minute)
        {
            var item = new NativeItem(name);

            item.Activated += (s, e) =>
            {
                _time.SetTime(hour, minute);
            };

            Menu.Add(item);
        }
    }
}