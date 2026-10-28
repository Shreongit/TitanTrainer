using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
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
        private readonly Dictionary<NativeMenu, NativeMenu> _menuParents =
            new Dictionary<NativeMenu, NativeMenu>();
        private NativeMenu _categoryMenu;
        private NativeMenu _controlsMenu;
        private NativeMenu _healthMenu;
        private NativeMenu _vehicleTogglesMenu;
        private NativeMenu _utilityMenu;
        private NativeMenu _doorsMenu;
        private NativeMenu _controlPerformanceMenu;
        private NativeMenu _wheelsMenu;
        private NativeMenu _wheelTypesMenu;
        private NativeMenu _frontWheelsMenu;
        private NativeMenu _rearWheelsMenu;
        private NativeMenu _windowsMenu;
        private NativeMenu _windowTintMenu;
        private NativeMenu _colorsMenu;
        private NativeMenu _bumpersMenu;
        private NativeMenu _performanceMenu;
        private NativeMenu _lightsMenu;
        private NativeMenu _neonColorsMenu;
        private NativeMenu _tireSmokeMenu;
        private NativeMenu _liveryMenu;
        private NativeMenu _otherModsMenu;
        private NativeMenu _presetMenu;
        private NativeMenu _savedPresetsMenu;
        private NativeMenu _favoritesMenu;
        private NativeMenu _customizationMenu;
        private NativeMenu _modSlotsMenu;
        private NativeMenu _searchResultsMenu;
        private NativeMenu _recentVehiclesMenu;
        private string _lastVehicleSearch = string.Empty;
        private readonly VehiclePresetService _presetService;

        public NativeMenu Menu { get; }
        public NativeMenu SpawnMenu { get; private set; }
        public bool RootWasClosedByBackspace { get; private set; }
        public void ClearRootBackspaceFlag() => RootWasClosedByBackspace = false;
        public IEnumerable<NativeMenu> AllMenus => _allMenus;

        public VehicleMenu(ServiceContainer services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _vehicleManager = _services.Resolve<VehicleManager>();
            _presetService = new VehiclePresetService(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data"),
                _services.Resolve<VehicleDatabase>(),
                _services.Resolve<NotificationService>());

            Menu = CreateMenu("Vehicles", "Vehicle controls, customization, and spawner");
            _controlsMenu = CreateMenu("Vehicle Controls", "Choose a vehicle-control category");
            _healthMenu = CreateMenu("Vehicle Health", "Repair and clean the current vehicle");
            _vehicleTogglesMenu = CreateMenu("Vehicle Toggles", "Enable or disable vehicle behaviors");
            _utilityMenu = CreateMenu("Vehicle Utility", "Flip, clone, delete, or spawn a personal vehicle");
            _doorsMenu = CreateMenu("Doors & Windows", "Open doors and operate vehicle windows");
            _controlPerformanceMenu = CreateMenu("Performance", "Upgrade current vehicle performance");
            SpawnMenu = CreateMenu("Vehicle Spawner", "Spawn and browse vehicles");
            BuildMenu();
            BuildSpawnMenu();
            BuildCategoryMenus();
            BuildPresetMenus();
            BuildSearchAndHistoryMenus();
            BuildCustomizationMenus();
            BuildRootMenu();
            ConfigureNavigationParents();
        }

        private NativeMenu CreateMenu(string title, string subtitle)
        {
            var menu = new NativeMenu(title, subtitle) { Visible = false };
            _allMenus.Add(menu);
            return menu;
        }

        private void ConfigureNavigationParents()
        {
            SetParent(_controlsMenu, Menu);
            SetParent(_healthMenu, _controlsMenu);
            SetParent(_vehicleTogglesMenu, _controlsMenu);
            SetParent(_utilityMenu, _controlsMenu);
            SetParent(_doorsMenu, _controlsMenu);
            SetParent(_controlPerformanceMenu, _controlsMenu);
            SetParent(_customizationMenu, Menu);
            SetParent(SpawnMenu, Menu);
            SetParent(_categoryMenu, SpawnMenu);
            foreach (var categoryMenu in _categoryMenus.Values)
                SetParent(categoryMenu, _categoryMenu);
            SetParent(_presetMenu, SpawnMenu);
            SetParent(_savedPresetsMenu, _presetMenu);
            SetParent(_favoritesMenu, _presetMenu);
            SetParent(_searchResultsMenu, SpawnMenu);
            SetParent(_recentVehiclesMenu, SpawnMenu);

            SetParent(_wheelsMenu, _customizationMenu);
            SetParent(_wheelTypesMenu, _wheelsMenu);
            SetParent(_frontWheelsMenu, _wheelsMenu);
            SetParent(_rearWheelsMenu, _wheelsMenu);
            SetParent(_windowsMenu, _customizationMenu);
            SetParent(_windowTintMenu, _windowsMenu);
            SetParent(_colorsMenu, _customizationMenu);
            SetParent(_bumpersMenu, _customizationMenu);
            SetParent(_performanceMenu, _customizationMenu);
            SetParent(_lightsMenu, _customizationMenu);
            SetParent(_neonColorsMenu, _lightsMenu);
            SetParent(_tireSmokeMenu, _lightsMenu);
            SetParent(_liveryMenu, _customizationMenu);
            SetParent(_otherModsMenu, _customizationMenu);
            SetParent(_modSlotsMenu, _otherModsMenu);
        }

        private void BuildRootMenu()
        {
            Menu.Clear();
            AddOpenMenuItem(Menu, "Vehicle Controls", "Repair, doors, vehicle toggles, and utility", _controlsMenu);
            AddOpenMenuItem(Menu, "Customization", "Wheels, windows, colors, bumpers, lights, and upgrades", _customizationMenu);
            AddOpenMenuItem(Menu, "Spawner", "Browse categories, search, recent vehicles, presets, and favorites", SpawnMenu);
        }

        private void AddOpenMenuItem(NativeMenu source, string title, string description, NativeMenu target)
        {
            var item = new NativeItem(title, description);
            item.Activated += (s, e) => { HideAll(); target.Visible = true; };
            source.Add(item);
        }

        private void SetParent(NativeMenu child, NativeMenu parent)
        {
            if (child != null && parent != null)
                _menuParents[child] = parent;
        }

        /// <summary>
        /// Handles keyboard Backspace for vehicle menus. Returns false when no vehicle
        /// menu is visible so other trainer menus can process their own navigation.
        /// </summary>
        public bool HandleBackspace()
        {
            NativeMenu visibleMenu = _allMenus.FirstOrDefault(menu => menu.Visible);
            if (visibleMenu == null)
                return false;

            RootWasClosedByBackspace = false;
            if (_menuParents.TryGetValue(visibleMenu, out NativeMenu parent))
            {
                HideAll();
                parent.Visible = true;
            }
            else
            {
                HideAll();
                if (visibleMenu == Menu) RootWasClosedByBackspace = true;
            }

            return true;
        }

        private void BuildMenu()
        {
            _controlsMenu.Clear();
            AddOpenMenuItem(_controlsMenu, "Vehicle Health", "Repair and clean the current vehicle", _healthMenu);
            AddOpenMenuItem(_controlsMenu, "Vehicle Toggles", "God mode, turbo, engine, seatbelt, and more", _vehicleTogglesMenu);
            AddOpenMenuItem(_controlsMenu, "Vehicle Utility", "Flip, clone, delete, or spawn a personal vehicle", _utilityMenu);
            AddOpenMenuItem(_controlsMenu, "Doors & Windows", "Operate doors and windows", _doorsMenu);
            AddOpenMenuItem(_controlsMenu, "Performance", "Upgrade the current vehicle", _controlPerformanceMenu);

            var repair = new NativeItem("Repair Vehicle", "Repair the current vehicle");
            repair.Activated += (s, e) => _vehicleManager.Health.Repair();
            _healthMenu.Add(repair);
            var clean = new NativeItem("Clean Vehicle", "Clean the current vehicle");
            clean.Activated += (s, e) => _vehicleManager.Health.Clean();
            _healthMenu.Add(clean);

            var god = new NativeCheckboxItem("Vehicle God Mode", false);
            god.CheckboxChanged += (s, e) => _vehicleManager.Health.GodMode = god.Checked;
            _vehicleTogglesMenu.Add(god);
            var autoRepair = new NativeCheckboxItem("Auto Repair", false);
            autoRepair.CheckboxChanged += (s, e) => _vehicleManager.Health.AutoRepair = autoRepair.Checked;
            _vehicleTogglesMenu.Add(autoRepair);
            var tires = new NativeCheckboxItem("Invincible Tires", false);
            tires.CheckboxChanged += (s, e) => _vehicleManager.Health.InvincibleTires = tires.Checked;
            _vehicleTogglesMenu.Add(tires);
            var turbo = new NativeCheckboxItem("Turbo", false);
            turbo.CheckboxChanged += (s, e) => _vehicleManager.Performance.Turbo = turbo.Checked;
            _vehicleTogglesMenu.Add(turbo);
            var boost = new NativeCheckboxItem("Boost", false);
            boost.CheckboxChanged += (s, e) => _vehicleManager.Performance.Boost = boost.Checked;
            _vehicleTogglesMenu.Add(boost);
            var rainbow = new NativeCheckboxItem("Rainbow Paint", false);
            rainbow.CheckboxChanged += (s, e) => _vehicleManager.Appearance.RainbowPaint = rainbow.Checked;
            _vehicleTogglesMenu.Add(rainbow);
            var neon = new NativeCheckboxItem("Neons", false);
            neon.CheckboxChanged += (s, e) => _vehicleManager.Appearance.Neons = neon.Checked;
            _vehicleTogglesMenu.Add(neon);
            var engine = new NativeCheckboxItem("Engine", true);
            engine.CheckboxChanged += (s, e) => _vehicleManager.Controls.Engine = engine.Checked;
            _vehicleTogglesMenu.Add(engine);
            var freeze = new NativeCheckboxItem("Freeze Vehicle", false);
            freeze.CheckboxChanged += (s, e) => _vehicleManager.Controls.FreezeVehicle = freeze.Checked;
            _vehicleTogglesMenu.Add(freeze);
            var seatbelt = new NativeCheckboxItem("Seatbelt", false);
            seatbelt.CheckboxChanged += (s, e) => _vehicleManager.Controls.Seatbelt = seatbelt.Checked;
            _vehicleTogglesMenu.Add(seatbelt);

            var flip = new NativeItem("Flip Vehicle");
            flip.Activated += (s, e) => _vehicleManager.Utility.FlipVehicle();
            _utilityMenu.Add(flip);
            var clone = new NativeItem("Clone Vehicle");
            clone.Activated += (s, e) => _vehicleManager.Utility.CloneVehicle();
            _utilityMenu.Add(clone);
            var delete = new NativeItem("Delete Vehicle");
            delete.Activated += (s, e) => _vehicleManager.Utility.DeleteVehicle();
            _utilityMenu.Add(delete);
            var personal = new NativeItem("Spawn Personal Vehicle");
            personal.Activated += (s, e) => _vehicleManager.Utility.SpawnPersonalVehicle();
            _utilityMenu.Add(personal);

            AddDoorAction("Toggle Driver Door", 0);
            AddDoorAction("Toggle Passenger Door", 1);
            AddDoorAction("Toggle Rear Left Door", 2);
            AddDoorAction("Toggle Rear Right Door", 3);
            AddDoorAction("Toggle Hood", 4);
            AddDoorAction("Toggle Trunk", 5);
            var allDoors = new NativeItem("Open All Doors");
            allDoors.Activated += (s, e) => SetAllDoors(true);
            _doorsMenu.Add(allDoors);
            var closeDoors = new NativeItem("Close All Doors");
            closeDoors.Activated += (s, e) => SetAllDoors(false);
            _doorsMenu.Add(closeDoors);
            var windowsDown = new NativeItem("Roll Down Windows");
            windowsDown.Activated += (s, e) => ForEachWindow(true);
            _doorsMenu.Add(windowsDown);
            var windowsUp = new NativeItem("Roll Up Windows");
            windowsUp.Activated += (s, e) => ForEachWindow(false);
            _doorsMenu.Add(windowsUp);

            var maxUpgrade = new NativeItem("Max Upgrade");
            maxUpgrade.Activated += (s, e) => _vehicleManager.Performance.UpgradeVehicle();
            _controlPerformanceMenu.Add(maxUpgrade);
        }

        private void BuildSpawnMenu()
        {
            SpawnMenu.Clear();
            var browse = new NativeItem("Browse Vehicles by Category", "Choose a category, then select a vehicle");
            browse.Activated += (s, e) =>
            {
                HideAll();
                _categoryMenu.Visible = true;
            };
            SpawnMenu.Add(browse);
            var searchVehicles = new NativeItem("Search Vehicles", "Search by display name, model name, or category");
            searchVehicles.Activated += (s, e) => SearchVehiclesFromPrompt();
            SpawnMenu.Add(searchVehicles);
            var recentVehicles = new NativeItem("Recently Spawned Vehicles", "Quickly spawn one of your recent vehicles again");
            recentVehicles.Activated += (s, e) => { RefreshRecentVehiclesMenu(); HideAll(); _recentVehiclesMenu.Visible = true; };
            SpawnMenu.Add(recentVehicles);
            var presets = new NativeItem("Vehicle Presets & Favorites", "Save, restore, and favorite vehicles");
            presets.Activated += (s, e) =>
            {
                RefreshPresetMenu();
                HideAll();
                _presetMenu.Visible = true;
            };
            SpawnMenu.Add(presets);

        }

        private void BuildCustomizationMenus()
        {
            _customizationMenu = CreateMenu("Customization", "Choose a customization category");
            _modSlotsMenu = CreateMenu("Vehicle Mod Slots", "Choose a supported modification slot");
            _wheelsMenu = CreateMenu("Wheels", "Wheel types and wheel designs");
            _wheelTypesMenu = CreateMenu("Wheel Type", "Choose the wheel category");
            _frontWheelsMenu = CreateMenu("Front Wheels", "Cycle front wheel designs");
            _rearWheelsMenu = CreateMenu("Rear Wheels", "Cycle rear wheel designs");
            _windowsMenu = CreateMenu("Windows", "Glass tint and window controls");
            _windowTintMenu = CreateMenu("Window Tint", "Choose the window tint");
            _colorsMenu = CreateMenu("Colors", "Paint presets and color effects");
            _bumpersMenu = CreateMenu("Bumpers", "Choose front and rear bumper modifications");
            _performanceMenu = CreateMenu("Performance", "Engine, brakes, transmission, suspension, and armor");
            _lightsMenu = CreateMenu("Lights & Effects", "Headlights, neons, and tire smoke");
            _neonColorsMenu = CreateMenu("Neon Color", "Choose an underglow color");
            _tireSmokeMenu = CreateMenu("Tire Smoke Color", "Choose a tire smoke color");
            _liveryMenu = CreateMenu("Livery", "Vehicle livery options");
            _otherModsMenu = CreateMenu("Other Modifications", "Additional vehicle-specific upgrade slots");

            AddOpenMenuItem(_customizationMenu, "Wheels", "Wheel type and front/rear wheel designs", _wheelsMenu);
            AddOpenMenuItem(_customizationMenu, "Windows", "Window tint and window options", _windowsMenu);
            AddOpenMenuItem(_customizationMenu, "Colors", "Paint presets and color effects", _colorsMenu);
            AddOpenMenuItem(_customizationMenu, "Bumpers", "Front and rear bumper options", _bumpersMenu);
            AddOpenMenuItem(_customizationMenu, "Performance", "Engine, brakes, transmission, suspension, and armor", _performanceMenu);
            AddOpenMenuItem(_customizationMenu, "Lights & Effects", "Xenon, neon, and tire smoke", _lightsMenu);
            AddOpenMenuItem(_customizationMenu, "Livery", "Cycle supported vehicle liveries", _liveryMenu);
            AddOpenMenuItem(_customizationMenu, "Other Modifications", "Open additional vehicle-specific slots", _otherModsMenu);

            AddWheelType("Sport Wheels", 0, _wheelTypesMenu);
            AddWheelType("Muscle Car Wheels", 1, _wheelTypesMenu);
            AddWheelType("Lowrider Wheels", 2, _wheelTypesMenu);
            AddWheelType("SUV Wheels", 3, _wheelTypesMenu);
            AddWheelType("Off-Road Wheels", 4, _wheelTypesMenu);
            AddWheelType("Tuner Wheels", 5, _wheelTypesMenu);
            AddWheelType("Bike Wheels", 6, _wheelTypesMenu);
            AddWheelType("High-End Wheels", 7, _wheelTypesMenu);
            AddCycleModItem(_frontWheelsMenu, "Cycle Front Wheel Design", 23);
            AddCycleModItem(_rearWheelsMenu, "Cycle Rear Wheel Design", 24);
            AddOpenMenuItem(_wheelsMenu, "Wheel Type", "Select a wheel category", _wheelTypesMenu);
            AddOpenMenuItem(_wheelsMenu, "Front Wheel Design", "Browse front wheel design options", _frontWheelsMenu);
            AddOpenMenuItem(_wheelsMenu, "Rear Wheel Design", "Browse rear wheel design options", _rearWheelsMenu);

            AddOpenMenuItem(_windowsMenu, "Window Tint", "Choose tint shade", _windowTintMenu);
            AddTint("No Tint", 0, _windowTintMenu);
            AddTint("Light Smoke", 1, _windowTintMenu);
            AddTint("Dark Smoke", 2, _windowTintMenu);
            AddTint("Limo Tint", 3, _windowTintMenu);
            AddTint("Green Tint", 4, _windowTintMenu);
            AddTint("Factory Tint", 5, _windowTintMenu);
            AddTint("Pure Black Tint", 6, _windowTintMenu);
            var rollDown = new NativeItem("Roll Down All Windows");
            rollDown.Activated += (s, e) => ForEachWindow(true);
            _windowsMenu.Add(rollDown);
            var rollUp = new NativeItem("Roll Up All Windows");
            rollUp.Activated += (s, e) => ForEachWindow(false);
            _windowsMenu.Add(rollUp);

            AddColorPreset(_colorsMenu, "Gloss White", 0);
            AddColorPreset(_colorsMenu, "Gloss Black", 1);
            AddColorPreset(_colorsMenu, "Racing Red", 2);
            AddColorPreset(_colorsMenu, "Electric Blue", 3);
            AddColorPreset(_colorsMenu, "Racing Green", 4);
            AddColorPreset(_colorsMenu, "Neon Purple", 6);
            var twoTone = new NativeItem("Red / Black Two-Tone");
            twoTone.Activated += (s, e) => _vehicleManager.Appearance.ApplyTwoTonePreset(2, 1);
            _colorsMenu.Add(twoTone);
            var clearPaint = new NativeItem("Clear Custom Paint");
            clearPaint.Activated += (s, e) => _vehicleManager.Appearance.ClearCustomPaint();
            _colorsMenu.Add(clearPaint);
            var rainbow = new NativeCheckboxItem("Rainbow Paint", false);
            rainbow.CheckboxChanged += (s, e) => _vehicleManager.Appearance.RainbowPaint = rainbow.Checked;
            _colorsMenu.Add(rainbow);

            AddCycleModItem(_bumpersMenu, "Front Bumper", 1);
            AddCycleModItem(_bumpersMenu, "Rear Bumper", 2);
            AddCycleModItem(_performanceMenu, "Engine Upgrade", 11);
            AddCycleModItem(_performanceMenu, "Brakes Upgrade", 12);
            AddCycleModItem(_performanceMenu, "Transmission Upgrade", 13);
            AddCycleModItem(_performanceMenu, "Suspension Upgrade", 15);
            AddCycleModItem(_performanceMenu, "Armor Upgrade", 16);
            var turbo = new NativeCheckboxItem("Turbo", false);
            turbo.CheckboxChanged += (s, e) => _vehicleManager.Performance.Turbo = turbo.Checked;
            _performanceMenu.Add(turbo);
            var boost = new NativeCheckboxItem("Boost", false);
            boost.CheckboxChanged += (s, e) => _vehicleManager.Performance.Boost = boost.Checked;
            _performanceMenu.Add(boost);
            var maxUpgrade = new NativeItem("Max Upgrade All");
            maxUpgrade.Activated += (s, e) => _vehicleManager.Performance.UpgradeVehicle();
            _performanceMenu.Add(maxUpgrade);

            var xenonOn = new NativeItem("Enable Xenon Headlights");
            xenonOn.Activated += (s, e) => _vehicleManager.Appearance.SetXenon(true, 0);
            _lightsMenu.Add(xenonOn);
            var xenonBlue = new NativeItem("Xenon Color: Blue");
            xenonBlue.Activated += (s, e) => _vehicleManager.Appearance.SetXenon(true, 2);
            _lightsMenu.Add(xenonBlue);
            var xenonPurple = new NativeItem("Xenon Color: Purple");
            xenonPurple.Activated += (s, e) => _vehicleManager.Appearance.SetXenon(true, 12);
            _lightsMenu.Add(xenonPurple);
            var xenonOff = new NativeItem("Disable Xenon Headlights");
            xenonOff.Activated += (s, e) => _vehicleManager.Appearance.SetXenon(false, 0);
            _lightsMenu.Add(xenonOff);
            AddOpenMenuItem(_lightsMenu, "Neon Color", "Choose underglow color", _neonColorsMenu);
            AddOpenMenuItem(_lightsMenu, "Tire Smoke Color", "Choose tire smoke color", _tireSmokeMenu);
            AddNeonColor("White", 0, _neonColorsMenu);
            AddNeonColor("Red", 2, _neonColorsMenu);
            AddNeonColor("Blue", 3, _neonColorsMenu);
            AddNeonColor("Green", 4, _neonColorsMenu);
            AddNeonColor("Purple", 6, _neonColorsMenu);
            AddNeonColor("Pink", 8, _neonColorsMenu);
            AddSmokeColor("White", 0, _tireSmokeMenu);
            AddSmokeColor("Red", 2, _tireSmokeMenu);
            AddSmokeColor("Blue", 3, _tireSmokeMenu);
            AddSmokeColor("Green", 4, _tireSmokeMenu);
            AddSmokeColor("Purple", 6, _tireSmokeMenu);

            var cycleLivery = new NativeItem("Cycle Livery");
            cycleLivery.Activated += (s, e) => _vehicleManager.Appearance.CycleLivery();
            _liveryMenu.Add(cycleLivery);
            var modSlots = new NativeItem("Vehicle-Specific Mod Slots", "Only slots supported by the current vehicle are shown");
            modSlots.Activated += (s, e) => { RefreshModSlotsMenu(); HideAll(); _modSlotsMenu.Visible = true; };
            _otherModsMenu.Add(modSlots);
            AddCycleModItem(_otherModsMenu, "Spoiler", 0);
            AddCycleModItem(_otherModsMenu, "Side Skirts", 3);
            AddCycleModItem(_otherModsMenu, "Exhaust", 4);
            AddCycleModItem(_otherModsMenu, "Hood", 7);
            AddCycleModItem(_otherModsMenu, "Roof", 10);
        }

        private void AddWheelType(string label, int wheelType, NativeMenu target)
        {
            var item = new NativeItem(label);
            item.Activated += (s, e) => _vehicleManager.Appearance.SetWheelType(wheelType);
            target.Add(item);
        }

        private void AddTint(string label, int tint, NativeMenu target)
        {
            var item = new NativeItem(label);
            item.Activated += (s, e) => _vehicleManager.Appearance.SetWindowTint(tint);
            target.Add(item);
        }

        private void AddNeonColor(string label, int index, NativeMenu target)
        {
            var item = new NativeItem(label);
            item.Activated += (s, e) => _vehicleManager.Appearance.SetNeonColor(index);
            target.Add(item);
        }

        private void AddSmokeColor(string label, int index, NativeMenu target)
        {
            var item = new NativeItem(label);
            item.Activated += (s, e) => _vehicleManager.Appearance.SetTyreSmokeColor(index);
            target.Add(item);
        }

        private void AddColorPreset(NativeMenu target, string label, int index)
        {
            var item = new NativeItem(label + " Paint");
            item.Activated += (s, e) => _vehicleManager.Appearance.ApplyColorPreset(index);
            target.Add(item);
        }

        private void AddCycleModItem(NativeMenu target, string label, int slot)
        {
            var item = new NativeItem(label, "Activate to cycle to the next supported option");
            item.Activated += (s, e) => _vehicleManager.Appearance.CycleMod(slot);
            target.Add(item);
        }

        private void RefreshModSlotsMenu()
        {
            _modSlotsMenu.Clear();
            var vehicle = Game.Player.Character?.CurrentVehicle;
            if (vehicle == null || !vehicle.Exists())
            {
                var noVehicle = new NativeItem("Enter a vehicle first", "Mod slots are vehicle-specific");
                noVehicle.Enabled = false;
                _modSlotsMenu.Add(noVehicle);
            }
            else
            {
                string[] names =
                {
                    "Spoilers", "Front Bumper", "Rear Bumper", "Side Skirts", "Exhaust", "Frame", "Grille", "Hood",
                    "Left Fender", "Right Fender", "Roof", "Engine", "Brakes", "Transmission", "Horns", "Suspension",
                    "Armor", "Unknown Slot 17", "Turbo Toggle (use Vehicle Toggles)", "Subwoofer", "Tire Smoke Toggle",
                    "Hydraulics Toggle", "Xenon Toggle", "Front Wheels", "Rear Wheels", "Plate Holders", "Vanity Plates",
                    "Interior Trim", "Ornaments", "Dashboard", "Dial Design", "Door Speakers", "Seats", "Steering Wheel",
                    "Shifter", "Plaques", "Speakers", "Trunk", "Hydraulics", "Engine Block", "Air Filter", "Struts",
                    "Arch Covers", "Aerials", "Exterior Trim", "Tank", "Windows", "Livery Mod", "Unknown Slot 47", "Unknown Slot 48"
                };
                bool hasMods = false;
                for (int slot = 0; slot < names.Length; slot++)
                {
                    int capturedSlot = slot;
                    // Toggle mods are controlled through dedicated toggles, not indexed mod lists.
                    if (slot == 18 || slot == 20 || slot == 21 || slot == 22) continue;
                    int count = _vehicleManager.Appearance.GetModCount(slot);
                    if (count <= 0) continue;
                    hasMods = true;
                    var item = new NativeItem(names[slot], count + " available options; activate to cycle");
                    item.Activated += (s, e) => _vehicleManager.Appearance.CycleMod(capturedSlot);
                    _modSlotsMenu.Add(item);
                }
                if (!hasMods)
                {
                    var none = new NativeItem("No standard mod slots available", "This vehicle may use a different customization system");
                    none.Enabled = false;
                    _modSlotsMenu.Add(none);
                }
            }
            var back = new NativeItem("Back", "Close vehicle menus");
            back.Activated += (s, e) => NavigateBackFrom(_modSlotsMenu);
            _modSlotsMenu.Add(back);
        }

        private void BuildSearchAndHistoryMenus()
        {
            _searchResultsMenu = CreateMenu("Vehicle Search Results", "Search results from the vehicle catalogue");
            _recentVehiclesMenu = CreateMenu("Recently Spawned Vehicles", "Most recently spawned vehicles first");
            BuildSearchResultsMenu(string.Empty);
            RefreshRecentVehiclesMenu();
        }

        private void SearchVehiclesFromPrompt()
        {
            string query;
            try
            {
                query = Game.GetUserInput(WindowTitle.EnterMessage60, _lastVehicleSearch ?? string.Empty, 32);
            }
            catch (Exception ex)
            {
                Logger.Error("Vehicle search prompt failed: " + ex);
                _services.Resolve<NotificationService>()?.NotifyError("Could not open vehicle search prompt.");
                return;
            }

            if (string.IsNullOrWhiteSpace(query))
            {
                _services.Resolve<NotificationService>()?.NotifyInfo("Vehicle search cancelled or empty.");
                return;
            }

            _lastVehicleSearch = query.Trim();
            BuildSearchResultsMenu(_lastVehicleSearch);
            HideAll();
            _searchResultsMenu.Visible = true;
        }

        private void BuildSearchResultsMenu(string query)
        {
            if (_searchResultsMenu == null) return;
            _searchResultsMenu.Clear();

            var database = _services.Resolve<VehicleDatabase>();
            var results = database.Search(query)
                .OrderBy(v => v.DisplayName, StringComparer.OrdinalIgnoreCase)
                .Take(100)
                .ToList();

            if (string.IsNullOrWhiteSpace(query))
            {
                var prompt = new NativeItem("Choose Search Vehicles to enter a query", "Search by vehicle name, model, or category");
                prompt.Enabled = false;
                _searchResultsMenu.Add(prompt);
            }
            else if (results.Count == 0)
            {
                var empty = new NativeItem("No matching vehicles", "Try a model name, display name, or category");
                empty.Enabled = false;
                _searchResultsMenu.Add(empty);
            }
            else
            {
                _searchResultsMenu.Add(new NativeItem("Query: " + query, results.Count + " result(s), maximum 100 shown") { Enabled = false });
                foreach (var result in results)
                {
                    var captured = result;
                    var item = new NativeItem(captured.DisplayName, captured.ModelName + " | " + captured.Category);
                    item.Activated += (s, e) => SpawnVehicle(captured);
                    _searchResultsMenu.Add(item);
                }
            }
            var searchAgain = new NativeItem("Search Again", "Enter a different search query");
            searchAgain.Activated += (s, e) => SearchVehiclesFromPrompt();
            _searchResultsMenu.Add(searchAgain);
            var back = new NativeItem("Back", "Close vehicle menus");
            back.Activated += (s, e) => NavigateBackFrom(_searchResultsMenu);
            _searchResultsMenu.Add(back);
        }

        private void RefreshRecentVehiclesMenu()
        {
            if (_recentVehiclesMenu == null) return;
            _recentVehiclesMenu.Clear();
            var history = _vehicleManager.GetSpawnHistory()
                .GroupBy(v => v.ModelName, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .Take(30)
                .ToList();
            if (history.Count == 0)
            {
                var empty = new NativeItem("No recently spawned vehicles", "Spawn a vehicle to populate this list");
                empty.Enabled = false;
                _recentVehiclesMenu.Add(empty);
            }
            else
            {
                foreach (var vehicle in history)
                {
                    var captured = vehicle;
                    var item = new NativeItem(captured.DisplayName, "Spawn again: " + captured.ModelName);
                    item.Activated += (s, e) => SpawnVehicle(captured);
                    _recentVehiclesMenu.Add(item);
                }
            }
            var back = new NativeItem("Back", "Close vehicle menus");
            back.Activated += (s, e) => NavigateBackFrom(_recentVehiclesMenu);
            _recentVehiclesMenu.Add(back);
        }

        private void BuildPresetMenus()
        {
            _presetMenu = CreateMenu("Vehicle Presets", "Save and restore your favorite setups");
            _savedPresetsMenu = CreateMenu("Saved Vehicle Presets", "Select a preset to load it");
            _favoritesMenu = CreateMenu("Favorite Vehicles", "Spawn or remove favorite vehicles");
            RefreshPresetMenu();
        }

        private void RefreshPresetMenu()
        {
            if (_presetMenu == null) return;
            _presetMenu.Clear();
            var save = new NativeItem("Save Current Vehicle Preset", "Save model, colors, damage, and installed mods");
            save.Activated += (s, e) =>
            {
                if (_presetService.SaveCurrentVehicle()) RefreshPresetMenu();
            };
            _presetMenu.Add(save);

            var favorite = new NativeItem("Add / Remove Current Vehicle Favorite", "Toggle the current vehicle in your favorites");
            favorite.Activated += (s, e) =>
            {
                if (_presetService.ToggleFavoriteCurrent()) RefreshPresetMenu();
            };
            _presetMenu.Add(favorite);

            var saved = new NativeItem("Saved Presets (" + _presetService.Presets.Count + ")");
            saved.Activated += (s, e) =>
            {
                RefreshSavedPresetsMenu();
                HideAll();
                _savedPresetsMenu.Visible = true;
            };
            _presetMenu.Add(saved);

            var favorites = new NativeItem("Favorites (" + _presetService.Favorites.Count + ")");
            favorites.Activated += (s, e) =>
            {
                RefreshFavoritesMenu();
                HideAll();
                _favoritesMenu.Visible = true;
            };
            _presetMenu.Add(favorites);

            var back = new NativeItem("Back", "Close vehicle menus");
            back.Activated += (s, e) => NavigateBackFrom(_presetMenu);
            _presetMenu.Add(back);
        }

        private void RefreshSavedPresetsMenu()
        {
            _savedPresetsMenu.Clear();
            if (_presetService.Presets.Count == 0)
            {
                var empty = new NativeItem("No saved presets", "Enter a vehicle and use Save Current Vehicle Preset");
                empty.Enabled = false;
                _savedPresetsMenu.Add(empty);
            }
            foreach (var preset in _presetService.Presets.ToList())
            {
                var captured = preset;
                var load = new NativeItem(captured.Name, "Load this vehicle and restore its saved setup");
                load.Activated += (s, e) => _presetService.SpawnPreset(captured.Id);
                _savedPresetsMenu.Add(load);
                var delete = new NativeItem("Delete: " + captured.Name, "Permanently remove this saved preset");
                delete.Activated += (s, e) =>
                {
                    _presetService.DeletePreset(captured.Id);
                    RefreshSavedPresetsMenu();
                    RefreshPresetMenu();
                };
                _savedPresetsMenu.Add(delete);
            }
            var back = new NativeItem("Back", "Close vehicle menus");
            back.Activated += (s, e) => NavigateBackFrom(_savedPresetsMenu);
            _savedPresetsMenu.Add(back);
        }

        private void RefreshFavoritesMenu()
        {
            _favoritesMenu.Clear();
            if (_presetService.Favorites.Count == 0)
            {
                var empty = new NativeItem("No favorite vehicles", "Enter a vehicle and add it to favorites");
                empty.Enabled = false;
                _favoritesMenu.Add(empty);
            }
            foreach (var favorite in _presetService.Favorites.ToList())
            {
                var captured = favorite;
                var spawn = new NativeItem(captured.Name, "Spawn this favorite vehicle");
                spawn.Activated += (s, e) => _presetService.SpawnFavorite(captured.Id);
                _favoritesMenu.Add(spawn);
                var remove = new NativeItem("Remove: " + captured.Name, "Remove from favorites");
                remove.Activated += (s, e) =>
                {
                    _presetService.RemoveFavorite(captured.Id);
                    RefreshFavoritesMenu();
                    RefreshPresetMenu();
                };
                _favoritesMenu.Add(remove);
            }
            var back = new NativeItem("Back", "Close vehicle menus");
            back.Activated += (s, e) => NavigateBackFrom(_favoritesMenu);
            _favoritesMenu.Add(back);
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

            var back = new NativeItem("Back", "Close vehicle menus");
            back.Activated += (s, e) => NavigateBackFrom(_categoryMenu);
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
            var backToCategories = new NativeItem("Back", "Close vehicle menus");
            backToCategories.Activated += (s, e) => NavigateBackFrom(categorySubmenu);
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
            _doorsMenu.Add(item);
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
                    RefreshRecentVehiclesMenu();
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

        private void NavigateBackFrom(NativeMenu current)
        {
            if (current != null && _menuParents.TryGetValue(current, out NativeMenu parent))
            {
                HideAll();
                parent.Visible = true;
            }
            else
            {
                HideAll();
            }
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

        public void ShowSpawner()
        {
            HideAll();
            SpawnMenu.Visible = true;
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
