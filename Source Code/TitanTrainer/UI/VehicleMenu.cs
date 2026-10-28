using System;
using System.Collections.Generic;
using System.Linq;
using GTA;
using GTA.Native;
using LemonUI.Menus;
using TitanTrainer.Core;
using TitanTrainer.Utils;
using TitanTrainer.Vehicles;

namespace TitanTrainer.UI
{
    /// <summary>
    /// Vehicle controls and a three-level vehicle browser:
    /// Vehicle Spawner -> Category -> Vehicles in that category.
    /// </summary>
    public sealed class VehicleMenu
    {
        private readonly ServiceContainer _services;
        private readonly VehicleManager _vehicleManager;
        private readonly List<NativeMenu> _allMenus = new List<NativeMenu>();
        private readonly Dictionary<string, NativeMenu> _categoryMenus =
            new Dictionary<string, NativeMenu>(StringComparer.OrdinalIgnoreCase);
        private NativeMenu _categoryMenu;

        public NativeMenu Menu { get; }
        public IEnumerable<NativeMenu> AllMenus => _allMenus;

        public VehicleMenu(ServiceContainer services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _vehicleManager = _services.Resolve<VehicleManager>();

            Menu = CreateMenu("Vehicle Spawner", "Vehicle controls and browsing");
            BuildMenu();
            BuildCategoryMenus();
        }

        private NativeMenu CreateMenu(string title, string subtitle)
        {
            var menu = new NativeMenu(title, subtitle) { Visible = false };
            _allMenus.Add(menu);
            return menu;
        }

        private void BuildMenu()
        {
            Menu.Clear();

            var repair = new NativeItem("Repair Vehicle", "Repair the current vehicle");
            repair.Activated += (s, e) => _vehicleManager.Health.Repair();
            Menu.Add(repair);

            var clean = new NativeItem("Clean Vehicle", "Clean the current vehicle");
            clean.Activated += (s, e) => _vehicleManager.Health.Clean();
            Menu.Add(clean);

            Menu.Add(new NativeSeparatorItem("Vehicle Toggles"));

            var god = new NativeCheckboxItem("Vehicle God Mode", false);
            god.CheckboxChanged += (s, e) => _vehicleManager.Health.GodMode = god.Checked;
            Menu.Add(god);

            var autoRepair = new NativeCheckboxItem("Auto Repair", false);
            autoRepair.CheckboxChanged += (s, e) => _vehicleManager.Health.AutoRepair = autoRepair.Checked;
            Menu.Add(autoRepair);

            var tires = new NativeCheckboxItem("Invincible Tires", false);
            tires.CheckboxChanged += (s, e) => _vehicleManager.Health.InvincibleTires = tires.Checked;
            Menu.Add(tires);

            var turbo = new NativeCheckboxItem("Turbo", false);
            turbo.CheckboxChanged += (s, e) => _vehicleManager.Performance.Turbo = turbo.Checked;
            Menu.Add(turbo);

            var boost = new NativeCheckboxItem("Boost", false);
            boost.CheckboxChanged += (s, e) => _vehicleManager.Performance.Boost = boost.Checked;
            Menu.Add(boost);

            var rainbow = new NativeCheckboxItem("Rainbow Paint", false);
            rainbow.CheckboxChanged += (s, e) => _vehicleManager.Appearance.RainbowPaint = rainbow.Checked;
            Menu.Add(rainbow);

            var neon = new NativeCheckboxItem("Neons", false);
            neon.CheckboxChanged += (s, e) => _vehicleManager.Appearance.Neons = neon.Checked;
            Menu.Add(neon);

            var engine = new NativeCheckboxItem("Engine", true);
            engine.CheckboxChanged += (s, e) => _vehicleManager.Controls.Engine = engine.Checked;
            Menu.Add(engine);

            var freeze = new NativeCheckboxItem("Freeze Vehicle", false);
            freeze.CheckboxChanged += (s, e) => _vehicleManager.Controls.FreezeVehicle = freeze.Checked;
            Menu.Add(freeze);

            var seatbelt = new NativeCheckboxItem("Seatbelt", false);
            seatbelt.CheckboxChanged += (s, e) => _vehicleManager.Controls.Seatbelt = seatbelt.Checked;
            Menu.Add(seatbelt);

            Menu.Add(new NativeSeparatorItem("Vehicle Utility"));

            var flip = new NativeItem("Flip Vehicle");
            flip.Activated += (s, e) => _vehicleManager.Utility.FlipVehicle();
            Menu.Add(flip);

            var clone = new NativeItem("Clone Vehicle");
            clone.Activated += (s, e) => _vehicleManager.Utility.CloneVehicle();
            Menu.Add(clone);

            var delete = new NativeItem("Delete Vehicle");
            delete.Activated += (s, e) => _vehicleManager.Utility.DeleteVehicle();
            Menu.Add(delete);

            var personal = new NativeItem("Spawn Personal Vehicle");
            personal.Activated += (s, e) => _vehicleManager.Utility.SpawnPersonalVehicle();
            Menu.Add(personal);

            Menu.Add(new NativeSeparatorItem("Doors & Windows"));
            AddDoorAction("Toggle Driver Door", 0);
            AddDoorAction("Toggle Passenger Door", 1);
            AddDoorAction("Toggle Rear Left Door", 2);
            AddDoorAction("Toggle Rear Right Door", 3);
            AddDoorAction("Toggle Hood", 4);
            AddDoorAction("Toggle Trunk", 5);
            var allDoors = new NativeItem("Open All Doors");
            allDoors.Activated += (s, e) => SetAllDoors(true);
            Menu.Add(allDoors);
            var closeDoors = new NativeItem("Close All Doors");
            closeDoors.Activated += (s, e) => SetAllDoors(false);
            Menu.Add(closeDoors);
            var windowsDown = new NativeItem("Roll Down Windows");
            windowsDown.Activated += (s, e) => ForEachWindow(true);
            Menu.Add(windowsDown);
            var windowsUp = new NativeItem("Roll Up Windows");
            windowsUp.Activated += (s, e) => ForEachWindow(false);
            Menu.Add(windowsUp);

            Menu.Add(new NativeSeparatorItem("Performance"));
            var maxUpgrade = new NativeItem("Max Upgrade");
            maxUpgrade.Activated += (s, e) => _vehicleManager.Performance.UpgradeVehicle();
            Menu.Add(maxUpgrade);

            Menu.Add(new NativeSeparatorItem("Vehicle Browser"));
            var browse = new NativeItem("Browse Vehicles by Category", "Choose a category, then select a vehicle");
            browse.Activated += (s, e) =>
            {
                HideAll();
                _categoryMenu.Visible = true;
            };
            Menu.Add(browse);
        }

        private void BuildCategoryMenus()
        {
            _categoryMenu = CreateMenu("Vehicle Categories", "Select a vehicle category");

            // Keep the requested categories visible even when the current catalogue has
            // no entries for them, and include the other categories that exist in JSON.
            var categoryDefinitions = new List<CategoryDefinition>
            {
                new CategoryDefinition("Super", new[] { "Super" }),
                new CategoryDefinition("Sports", new[] { "Sports" }),
                new CategoryDefinition("Bikes", new[] { "Motorcycle", "Motorcycles", "Bike", "Bikes" }),
                new CategoryDefinition("Trailers", new[] { "Trailer", "Trailers" }),
                new CategoryDefinition("Trucks", new[] { "Truck", "Trucks" }),
                new CategoryDefinition("Planes", new[] { "Plane", "Planes" }),
                new CategoryDefinition("Boats", new[] { "Boat", "Boats" }),
                new CategoryDefinition("Helicopters", new[] { "Helicopter", "Helicopters" }),
                new CategoryDefinition("Muscle", new[] { "Muscle" }),
                new CategoryDefinition("Sports Classics", new[] { "Sports Classic", "Sports Classics" }),
                new CategoryDefinition("Off-Road", new[] { "Off-Road", "Off Road" }),
                new CategoryDefinition("SUVs", new[] { "SUV", "SUVs" }),
                new CategoryDefinition("Sedans", new[] { "Sedan", "Sedans" }),
                new CategoryDefinition("Vans", new[] { "Van", "Vans" }),
                new CategoryDefinition("Open Wheel", new[] { "Open Wheel", "Open-Wheel" })
            };

            var database = _services.Resolve<VehicleDatabase>();
            foreach (var definition in categoryDefinitions)
            {
                AddCategory(definition, database);
            }

            // Preserve support for any additional categories in a user-supplied vehicles.json.
            var knownAliases = new HashSet<string>(categoryDefinitions.SelectMany(c => c.SourceCategories.Select(Normalize)),
                StringComparer.OrdinalIgnoreCase);
            foreach (var category in database.GetCategories())
            {
                if (knownAliases.Contains(Normalize(category))) continue;
                AddCategory(new CategoryDefinition(category, new[] { category }), database);
            }

            var back = new NativeItem("Back", "Return to Vehicle Spawner");
            back.Activated += (s, e) =>
            {
                HideAll();
                Menu.Visible = true;
            };
            _categoryMenu.Add(new NativeSeparatorItem("Navigation"));
            _categoryMenu.Add(back);
        }

        private void AddCategory(CategoryDefinition definition, VehicleDatabase database)
        {
            var matchingVehicles = database.AllVehicles
                .Where(v => definition.SourceCategories.Any(c =>
                    string.Equals(Normalize(c), Normalize(v.Category), StringComparison.OrdinalIgnoreCase)))
                .OrderBy(v => v.DisplayName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var categorySubmenu = CreateMenu(definition.DisplayName, "Select a vehicle to spawn");
            _categoryMenus[definition.DisplayName] = categorySubmenu;

            var categoryItem = new NativeItem(
                definition.DisplayName,
                matchingVehicles.Count == 0
                    ? "No vehicles currently listed in this category"
                    : matchingVehicles.Count + " vehicles");
            categoryItem.Activated += (s, e) =>
            {
                HideAll();
                categorySubmenu.Visible = true;
            };
            _categoryMenu.Add(categoryItem);

            if (matchingVehicles.Count == 0)
            {
                var empty = new NativeItem("No vehicles available", "Add models to Data/vehicles.json for this category");
                empty.Enabled = false;
                categorySubmenu.Add(empty);
            }
            else
            {
                foreach (var vehicle in matchingVehicles)
                {
                    var capturedVehicle = vehicle;
                    var item = new NativeItem(capturedVehicle.DisplayName, capturedVehicle.ModelName);
                    item.Activated += (sender, args) => SpawnVehicle(capturedVehicle);
                    categorySubmenu.Add(item);
                }
            }

            categorySubmenu.Add(new NativeSeparatorItem("Navigation"));
            var backToCategories = new NativeItem("Back", "Return to vehicle categories");
            backToCategories.Activated += (s, e) =>
            {
                HideAll();
                _categoryMenu.Visible = true;
            };
            categorySubmenu.Add(backToCategories);
        }

        private void AddDoorAction(string label, int doorIndex)
        {
            var item = new NativeItem(label);
            item.Activated += (s, e) =>
            {
                var vehicle = Game.Player.Character.CurrentVehicle;
                if (vehicle == null || !vehicle.Exists())
                {
                    _services.Resolve<NotificationService>()?.NotifyError("Enter a vehicle first.");
                    return;
                }
                if (Function.Call<float>(Hash.GET_VEHICLE_DOOR_ANGLE_RATIO, vehicle.Handle, doorIndex) > 0.1f)
                    Function.Call(Hash.SET_VEHICLE_DOOR_SHUT, vehicle.Handle, doorIndex, false);
                else
                    Function.Call(Hash.SET_VEHICLE_DOOR_OPEN, vehicle.Handle, doorIndex, false, false);
            };
            Menu.Add(item);
        }

        private void SetAllDoors(bool open)
        {
            var vehicle = Game.Player.Character.CurrentVehicle;
            if (vehicle == null || !vehicle.Exists())
            {
                _services.Resolve<NotificationService>()?.NotifyError("Enter a vehicle first.");
                return;
            }
            for (int i = 0; i < 6; i++)
            {
                if (open) Function.Call(Hash.SET_VEHICLE_DOOR_OPEN, vehicle.Handle, i, false, false);
                else Function.Call(Hash.SET_VEHICLE_DOOR_SHUT, vehicle.Handle, i, false);
            }
        }

        private void ForEachWindow(bool down)
        {
            var vehicle = Game.Player.Character.CurrentVehicle;
            if (vehicle == null || !vehicle.Exists())
            {
                _services.Resolve<NotificationService>()?.NotifyError("Enter a vehicle first.");
                return;
            }
            for (int i = 0; i < 8; i++)
            {
                if (down) Function.Call(Hash.ROLL_DOWN_WINDOW, vehicle.Handle, i);
                else Function.Call(Hash.ROLL_UP_WINDOW, vehicle.Handle, i);
            }
        }

        private void SpawnVehicle(VehicleDefinition vehicle)
        {
            try
            {
                var config = _services.Resolve<Config>();
                bool spawnInside = config.SpawnDefaults.SpawnInside;
                bool maxUpgrade = config.SpawnDefaults.MaxUpgrade;

                if (_vehicleManager.SpawnVehicle(vehicle, spawnInside, maxUpgrade))
                {
                    _services.Resolve<NotificationService>()?.NotifySuccess("Spawned " + vehicle.DisplayName);
                }
                else
                {
                    _services.Resolve<NotificationService>()?.NotifyError("Could not spawn " + vehicle.DisplayName);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Vehicle spawn failed for " + vehicle.ModelName + ": " + ex);
                _services.Resolve<NotificationService>()?.NotifyError("Vehicle spawn failed.");
            }
        }

        private static string Normalize(string value)
        {
            return (value ?? string.Empty).Trim().Replace('_', ' ').Replace('-', ' ');
        }

        private void HideAll()
        {
            foreach (var menu in _allMenus)
                menu.Visible = false;
        }

        public void Show()
        {
            HideAll();
            Menu.Visible = true;
        }

        public void Hide()
        {
            HideAll();
        }

        private sealed class CategoryDefinition
        {
            public string DisplayName { get; }
            public string[] SourceCategories { get; }

            public CategoryDefinition(string displayName, string[] sourceCategories)
            {
                DisplayName = displayName;
                SourceCategories = sourceCategories ?? new string[0];
            }
        }
    }
}
