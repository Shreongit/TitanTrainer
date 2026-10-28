using GTA.Math;
using LemonUI.Menus;
using TitanTrainer.Player;
using TitanTrainer.Utils;

namespace TitanTrainer.UI
{
    /// <summary>Quick travel locations for GTA V Story Mode.</summary>
    public sealed class TeleportMenu : BaseMenu
    {
        private readonly TeleportManager _teleport;
        private readonly NotificationService _notifications;
        private readonly System.Action _goBack;

        public TeleportMenu(TeleportManager teleport, NotificationService notifications, System.Action goBack)
            : base("Titan Trainer", "Teleport & Locations")
        {
            _teleport = teleport;
            _notifications = notifications;
            _goBack = goBack;

            AddAction("Teleport to Waypoint", "Use the map waypoint", () => _teleport.TeleportToWaypoint());
            AddAction("Teleport Forward", "Move 10 metres forward", () => _teleport.TeleportForward(10f));
            AddAction("Safe Lift", "Move slightly upward", () => _teleport.TeleportSafe());
            AddAction("Sky Drop", "Move high above the current position", () => _teleport.TeleportSky());

            Menu.Add(new NativeSeparatorItem("Los Santos & Blaine County"));
            AddLocation("Los Santos International Airport", new Vector3(-1034.6f, -2733.6f, 20.2f));
            AddLocation("Legion Square", new Vector3(195.2f, -933.8f, 30.7f));
            AddLocation("Maze Bank Tower", new Vector3(-75.0f, -818.9f, 326.2f));
            AddLocation("Vespucci Beach", new Vector3(-1165.0f, -1520.0f, 10.6f));
            AddLocation("Sandy Shores Airfield", new Vector3(1737.5f, 3288.0f, 41.1f));
            AddLocation("Mount Chiliad Summit", new Vector3(501.0f, 5604.0f, 797.9f));
            AddLocation("Fort Zancudo Entrance", new Vector3(-2047.0f, 3132.0f, 32.8f));
            AddLocation("Paleto Bay", new Vector3(-95.0f, 6457.0f, 31.5f));

            Menu.Add(new NativeSeparatorItem("Navigation"));
            var back = new NativeItem("Back", "Return to the main menu");
            back.Activated += (s, e) => { Hide(); _goBack?.Invoke(); };
            Menu.Add(back);
        }

        private void AddAction(string name, string description, System.Action action)
        {
            var item = new NativeItem(name, description);
            item.Activated += (s, e) => action?.Invoke();
            Menu.Add(item);
        }

        private void AddLocation(string name, Vector3 position)
        {
            var item = new NativeItem(name, "Teleport to saved coordinates");
            item.Activated += (s, e) =>
            {
                _teleport.TeleportTo(position);
                _notifications?.NotifyInfo("Location: " + name);
            };
            Menu.Add(item);
        }
    }
}
