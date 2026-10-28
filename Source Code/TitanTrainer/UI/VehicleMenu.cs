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
        private NativeMenu _categoryMenu;
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
        public IEnumerable<NativeMenu> AllMenus => _allMenus;

        public VehicleMenu(ServiceContainer services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _vehicleManager = _services.Resolve<VehicleManager>();
            _presetService = new VehiclePresetService(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data"),
                _services.Resolve<VehicleDatabase>(),
                _services.Resolve<NotificationService>());

            Menu = CreateMenu("Vehicle Spawner", "Vehicle controls and browsing");
            BuildMenu();
            BuildCategoryMenus();
            BuildPresetMenus();
            BuildSearchAndHistoryMenus();
            BuildCustomizationMenus();
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

            Menu.Add(new NativeSeparatorItem("Search & History"));
            var searchVehicles = new NativeItem("Search Vehicles", "Search by display name, model name, or category");
            searchVehicles.Activated += (s, e) => SearchVehiclesFromPrompt();
            Menu.Add(searchVehicles);
            var recentVehicles = new NativeItem("Recently Spawned Vehicles", "Quickly spawn one of your recent vehicles again");
            recentVehicles.Activated += (s, e) => { RefreshRecentVehiclesMenu(); HideAll(); _recentVehiclesMenu.Visible = true; };
            Menu.Add(recentVehicles);

            Menu.Add(new NativeSeparatorItem("Presets & Favorites"));
            var presets = new NativeItem("Vehicle Presets & Favorites", "Save, restore, and favorite vehicles");
            presets.Activated += (s, e) =>
            {
                RefreshPresetMenu();
                HideAll();
                _presetMenu.Visible = true;
            };
            Menu.Add(presets);

            Menu.Add(new NativeSeparatorItem("Advanced Customization"));
            var customization = new NativeItem("Advanced Vehicle Customization", "Paint, wheels, lights, liveries, and individual mods");
            customization.Activated += (s, e) => { HideAll(); _customizationMenu.Visible = true; };
            Menu.Add(customization);
        }



        private void BuildCustomizationMenus()
        {
            _customizationMenu = CreateMenu("Advanced Vehicle Customization", "Customize the vehicle you are currently driving");
            _modSlotsMenu = CreateMenu("Vehicle Mod Slots", "Select a slot to cycle its available modifications");

            var paint = new NativeItem("Apply Gloss White Paint");
            paint.Activated += (s, e) => _vehicleManager.Appearance.ApplyColorPreset(0);
            _customizationMenu.Add(paint);
            var black = new NativeItem("Apply Gloss Black Paint");
            black.Activated += (s, e) => _vehicleManager.Appearance.ApplyColorPreset(1);
            _customizationMenu.Add(black);
            var red = new NativeItem("Apply Racing Red Paint");
            red.Activated += (s, e) => _vehicleManager.Appearance.ApplyColorPreset(2);
            _customizationMenu.Add(red);
            var blue = new NativeItem("Apply Electric Blue Paint");
            blue.Activated += (s, e) => _vehicleManager.Appearance.ApplyColorPreset(3);
            _customizationMenu.Add(blue);
            var green = new NativeItem("Apply Racing Green Paint");
            green.Activated += (s, e) => _vehicleManager.Appearance.ApplyColorPreset(4);
            _customizationMenu.Add(green);
            var purple = new NativeItem("Apply Neon Purple Paint");
            purple.Activated += (s, e) => _vehicleManager.Appearance.ApplyColorPreset(6);
            _customizationMenu.Add(purple);
            var twoTone = new NativeItem("Apply Red / Black Two-Tone");
            twoTone.Activated += (s, e) => _vehicleManager.Appearance.ApplyTwoTonePreset(2, 1);
            _customizationMenu.Add(twoTone);
            var clearPaint = new NativeItem("Clear Custom Paint");
            clearPaint.Activated += (s, e) => _vehicleManager.Appearance.ClearCustomPaint();
            _customizationMenu.Add(clearPaint);

            _customizationMenu.Add(new NativeSeparatorItem("Wheel Categories"));
            AddWheelType("Sport Wheels", 0);
            AddWheelType("Muscle Car Wheels", 1);
            AddWheelType("Lowrider Wheels", 2);
            AddWheelType("SUV Wheels", 3);
            AddWheelType("Off-Road Wheels", 4);
            AddWheelType("Tuner Wheels", 5);
            AddWheelType("Bike Wheels", 6);
            AddWheelType("High-End Wheels", 7);

            _customizationMenu.Add(new NativeSeparatorItem("Glass & Lighting"));
            AddTint("No Tint", 0);
            AddTint("Light Smoke Tint", 1);
            AddTint("Dark Smoke Tint", 2);
            AddTint("Limo Tint", 3);
            AddTint("Green Tint", 4);
            AddTint("Factory Tint", 5);
            AddTint("Pure Black Tint", 6);
            var xenon = new NativeItem("Enable Xenon Headlights");
            xenon.Activated += (s, e) => _vehicleManager.Appearance.SetXenon(true, 0);
            _customizationMenu.Add(xenon);
            var xenonBlue = new NativeItem("Xenon Color: Blue");
            xenonBlue.Activated += (s, e) => _vehicleManager.Appearance.SetXenon(true, 2);
            _customizationMenu.Add(xenonBlue);
            var xenonPurple = new NativeItem("Xenon Color: Purple");
            xenonPurple.Activated += (s, e) => _vehicleManager.Appearance.SetXenon(true, 12);
            _customizationMenu.Add(xenonPurple);
            var xenonOff = new NativeItem("Disable Xenon Headlights");
            xenonOff.Activated += (s, e) => _vehicleManager.Appearance.SetXenon(false, 0);
            _customizationMenu.Add(xenonOff);

            _customizationMenu.Add(new NativeSeparatorItem("Effects & Livery"));
            AddNeonColor("Neon: White", 0);
            AddNeonColor("Neon: Red", 2);
            AddNeonColor("Neon: Blue", 3);
            AddNeonColor("Neon: Green", 4);
            AddNeonColor("Neon: Purple", 6);
            AddNeonColor("Neon: Pink", 8);
            AddSmokeColor("Tire Smoke: White", 0);
            AddSmokeColor("Tire Smoke: Red", 2);
            AddSmokeColor("Tire Smoke: Blue", 3);
            AddSmokeColor("Tire Smoke: Green", 4);
            AddSmokeColor("Tire Smoke: Purple", 6);
            var livery = new NativeItem("Cycle Vehicle Livery");
            livery.Activated += (s, e) => _vehicleManager.Appearance.CycleLivery();
            _customizationMenu.Add(livery);

            var modSlots = new NativeItem("Individual Mod Slots", "Open only the modification slots supported by this vehicle");
            modSlots.Activated += (s, e) => { RefreshModSlotsMenu(); HideAll(); _modSlotsMenu.Visible = true; };
            _customizationMenu.Add(new NativeSeparatorItem("Vehicle-Specific Modifications"));
            _customizationMenu.Add(modSlots);

            _customizationMenu.Add(new NativeSeparatorItem("Navigation"));
            var back = new NativeItem("Back", "Return to Vehicle Spawner");
            back.Activated += (s, e) => { HideAll(); Menu.Visible = true; };
            _customizationMenu.Add(back);

            var modBack = new NativeItem("Back", "Return to Advanced Vehicle Customization");
            modBack.Activated += (s, e) => { HideAll(); _customizationMenu.Visible = true; };
            _modSlotsMenu.Add(new NativeSeparatorItem("Navigation"));
            _modSlotsMenu.Add(modBack);
        }

        private void AddWheelType(string label, int wheelType)
        {
            var item = new NativeItem(label);
            item.Activated += (s, e) => _vehicleManager.Appearance.SetWheelType(wheelType);
            _customizationMenu.Add(item);
        }

        private void AddTint(string label, int tint)
        {
            var item = new NativeItem(label);
            item.Activated += (s, e) => _vehicleManager.Appearance.SetWindowTint(tint);
            _customizationMenu.Add(item);
        }

        private void AddNeonColor(string label, int index)
        {
            var item = new NativeItem(label);
            item.Activated += (s, e) => _vehicleManager.Appearance.SetNeonColor(index);
            _customizationMenu.Add(item);
        }

        private void AddSmokeColor(string label, int index)
        {
            var item = new NativeItem(label);
            item.Activated += (s, e) => _vehicleManager.Appearance.SetTyreSmokeColor(index);
            _customizationMenu.Add(item);
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
            var back = new NativeItem("Back", "Return to Advanced Vehicle Customization");
            back.Activated += (s, e) => { HideAll(); _customizationMenu.Visible = true; };
            _modSlotsMenu.Add(new NativeSeparatorItem("Navigation"));
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
                query = Game.GetUserInput(WindowTitle.FMMC_KEY_TIP8, _lastVehicleSearch ?? string.Empty, 32);
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

            _searchResultsMenu.Add(new NativeSeparatorItem("Search"));
            var searchAgain = new NativeItem("Search Again", "Enter a different search query");
            searchAgain.Activated += (s, e) => SearchVehiclesFromPrompt();
            _searchResultsMenu.Add(searchAgain);
            var back = new NativeItem("Back", "Return to Vehicle Spawner");
            back.Activated += (s, e) => { HideAll(); Menu.Visible = true; };
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
            var back = new NativeItem("Back", "Return to Vehicle Spawner");
            back.Activated += (s, e) => { HideAll(); Menu.Visible = true; };
            _recentVehiclesMenu.Add(new NativeSeparatorItem("Navigation"));
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

            var back = new NativeItem("Back", "Return to Vehicle Spawner");
            back.Activated += (s, e) => { HideAll(); Menu.Visible = true; };
            _presetMenu.Add(new NativeSeparatorItem("Navigation"));
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
            var back = new NativeItem("Back", "Return to Vehicle Presets");
            back.Activated += (s, e) => { HideAll(); _presetMenu.Visible = true; };
            _savedPresetsMenu.Add(new NativeSeparatorItem("Navigation"));
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
            var back = new NativeItem("Back", "Return to Vehicle Presets");
            back.Activated += (s, e) => { HideAll(); _presetMenu.Visible = true; };
            _favoritesMenu.Add(new NativeSeparatorItem("Navigation"));
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
