using System;
using System.Linq;
using GTA;
using LemonUI.Menus;
using TitanTrainer.Core;
using TitanTrainer.Utils;
using TitanTrainer.Vehicles;

namespace TitanTrainer.UI
{
    /// <summary>
    /// Vehicle spawner menu.
    /// </summary>
    public class VehicleMenu
    {
        private readonly ServiceContainer _services;
        private readonly VehicleManager _vehicleManager;

        public NativeMenu Menu { get; }

        public VehicleMenu(ServiceContainer services)
        {
            _services = services;
            _vehicleManager = _services.Resolve<VehicleManager>();

            Menu = new NativeMenu("Vehicle Spawner", "Select a Vehicle");
            Menu.Visible = false;

            BuildMenu();
        }
        
        private void BuildMenu()
{
    Menu.Clear();

    //--------------------------------------------------
    // Vehicle Cheats
    //--------------------------------------------------

    var repair = new NativeItem("Repair Vehicle");
    repair.Activated += (s, e) => _vehicleManager.Health.Repair();
    Menu.Add(repair);

    var clean = new NativeItem("Clean Vehicle");
    clean.Activated += (s, e) => _vehicleManager.Health.Clean();
    Menu.Add(clean);

    Menu.Add(new NativeSeparatorItem("Vehicle Toggles"));

    var god = new NativeCheckboxItem("Vehicle God Mode", false);
    god.CheckboxChanged += (s, e) =>
    {
        _vehicleManager.Health.GodMode = god.Checked;
    };
    Menu.Add(god);

    var autoRepair = new NativeCheckboxItem("Auto Repair", false);
    autoRepair.CheckboxChanged += (s, e) =>
    {
        _vehicleManager.Health.AutoRepair = autoRepair.Checked;
    };
    Menu.Add(autoRepair);

    var tires = new NativeCheckboxItem("Invincible Tires", false);
    tires.CheckboxChanged += (s, e) =>
    {
        _vehicleManager.Health.InvincibleTires = tires.Checked;
    };
    Menu.Add(tires);

    var turbo = new NativeCheckboxItem("Turbo", false);
    turbo.CheckboxChanged += (s, e) =>
    {
        _vehicleManager.Performance.Turbo = turbo.Checked;
    };
    Menu.Add(turbo);

    var boost = new NativeCheckboxItem("Boost", false);
    boost.CheckboxChanged += (s, e) =>
    {
        _vehicleManager.Performance.Boost = boost.Checked;
    };
    Menu.Add(boost);

    var rainbow = new NativeCheckboxItem("Rainbow Paint", false);
    rainbow.CheckboxChanged += (s, e) =>
    {
        _vehicleManager.Appearance.RainbowPaint = rainbow.Checked;
    };
    Menu.Add(rainbow);

    var neon = new NativeCheckboxItem("Neons", false);
    neon.CheckboxChanged += (s, e) =>
    {
        _vehicleManager.Appearance.Neons = neon.Checked;
    };
    Menu.Add(neon);

    var engine = new NativeCheckboxItem("Engine", true);
    engine.CheckboxChanged += (s, e) =>
    {
        _vehicleManager.Controls.Engine = engine.Checked;
    };
    Menu.Add(engine);

    var freeze = new NativeCheckboxItem("Freeze Vehicle", false);
    freeze.CheckboxChanged += (s, e) =>
    {
        _vehicleManager.Controls.FreezeVehicle = freeze.Checked;
    };
    Menu.Add(freeze);

    var seatbelt = new NativeCheckboxItem("Seatbelt", false);
    seatbelt.CheckboxChanged += (s, e) =>
    {
        _vehicleManager.Controls.Seatbelt = seatbelt.Checked;
    };
    Menu.Add(seatbelt);

    Menu.Add(new NativeSeparatorItem("Vehicle Utility"));

    var flip = new NativeItem("Flip Vehicle");
    flip.Activated += (s, e) =>
    {
        _vehicleManager.Utility.FlipVehicle();
    };
    Menu.Add(flip);

    var clone = new NativeItem("Clone Vehicle");
    clone.Activated += (s, e) =>
    {
        _vehicleManager.Utility.CloneVehicle();
    };
    Menu.Add(clone);

    var delete = new NativeItem("Delete Vehicle");
    delete.Activated += (s, e) =>
    {
        _vehicleManager.Utility.DeleteVehicle();
    };
    Menu.Add(delete);

    var personal = new NativeItem("Spawn Personal Vehicle");
    personal.Activated += (s, e) =>
    {
        _vehicleManager.Utility.SpawnPersonalVehicle();
    };
    Menu.Add(personal);

    Menu.Add(new NativeSeparatorItem("Performance"));

    var maxUpgrade = new NativeItem("Max Upgrade");
    maxUpgrade.Activated += (s, e) =>
    {
        _vehicleManager.Performance.UpgradeVehicle();
    };
    Menu.Add(maxUpgrade);

    Menu.Add(new NativeSeparatorItem("Vehicle Spawner"));

    var vehicles = _vehicleManager.Database
        .Search("")
        .OrderBy(v => v.Category)
        .ThenBy(v => v.DisplayName)
        .ToList();

    string currentCategory = "";

    foreach (var vehicle in vehicles)
    {
        if (currentCategory != vehicle.Category)
        {
            currentCategory = vehicle.Category;

            var header = new NativeItem(
                $"========== {currentCategory.ToUpper()} ==========",
                ""
            );

            header.Enabled = false;

            Menu.Add(header);
        }

        var item = new NativeItem(
            vehicle.DisplayName,
            vehicle.ModelName);

        item.Activated += (sender, args) =>
        {
            try
            {
                bool spawnInside = _services
                    .Resolve<Config>()
                    .SpawnDefaults
                    .SpawnInside;

                _vehicleManager.SpawnVehicle(
                    vehicle,
                    spawnInside);

                _services.Resolve<NotificationService>()
                    ?.NotifySuccess(
                        $"Spawned {vehicle.DisplayName}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());

                _services.Resolve<NotificationService>()
                    ?.NotifyError(
                        "Vehicle spawn failed.");
            }
        };

        Menu.Add(item);
    }
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