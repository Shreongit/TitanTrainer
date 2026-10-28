using System;
using System.Collections.Generic;
using System.Linq;
using GTA;
using LemonUI.Menus;
using TitanTrainer.Utils;
using TitanTrainer.World;

namespace TitanTrainer.UI
{
    /// <summary>Prop and pedestrian spawning menus with explicit category navigation.</summary>
    public sealed class EntitySpawnerMenu : BaseMenu
    {
        private readonly EntitySpawner _spawner;
        private readonly NativeMenu _props;
        private readonly NativeMenu _peds;
        private readonly ObjectPlacementMenu _placementMenu;
        private readonly NativeMenu _pedManagement;
        private readonly NativeMenu _searchResults;
        private readonly Action _goBack;

        private readonly List<NativeMenu> _allMenus = new List<NativeMenu>();
        public NativeMenu[] AllMenus => _allMenus.ToArray();

        public EntitySpawnerMenu(EntitySpawner spawner, Action goBack)
            : base("Titan Trainer", "Entity Spawner")
        {
            _spawner = spawner;
            _goBack = goBack;
            _props = new NativeMenu("Titan Trainer", "Prop Spawner");
            _peds = new NativeMenu("Titan Trainer", "Pedestrian Spawner");
            _pedManagement = new NativeMenu("Titan Trainer", "Ped Management");
            _searchResults = new NativeMenu("Entity Search Results", "Matching props and pedestrian models");
            _placementMenu = new ObjectPlacementMenu(spawner, () => { HideAll(); Show(); });
            _allMenus.AddRange(new[] { Menu, _props, _peds, _pedManagement, _searchResults });
            _allMenus.AddRange(_placementMenu.AllMenus);
            _props.Visible = false;
            _peds.Visible = false;
            _searchResults.Visible = false;
            var propConstruction = MenuNavigation.CreateChild(_props, "Construction & Barriers", "Barrels, cones, and road barriers", _allMenus);
            var propFurniture = MenuNavigation.CreateChild(_props, "Furniture & Leisure", "Benches, chairs, tables, and umbrellas", _allMenus);
            var propContainers = MenuNavigation.CreateChild(_props, "Containers & Objects", "Crates, briefcases, and pallets", _allMenus);
            var propManagement = MenuNavigation.CreateChild(_props, "Prop Management", "Delete props spawned by this trainer", _allMenus);
            var lastPedManagement = MenuNavigation.CreateChild(_pedManagement, "Last Spawned Pedestrian", "Control the most recently spawned pedestrian", _allMenus);
            var allPedManagement = MenuNavigation.CreateChild(_pedManagement, "All Trainer Pedestrians", "Apply actions to every pedestrian spawned by Titan Trainer", _allMenus);
            MenuNavigation.AddBack(lastPedManagement, _pedManagement, _allMenus); MenuNavigation.AddBack(allPedManagement, _pedManagement, _allMenus);
            var pedCivilians = MenuNavigation.CreateChild(_peds, "Civilians", "Civilian pedestrian models", _allMenus);
            var pedServices = MenuNavigation.CreateChild(_peds, "Emergency & Service Peds", "Police, paramedics, firefighters, and mechanics", _allMenus);
            var pedActivities = MenuNavigation.CreateChild(_peds, "Activity & Outdoor Peds", "Tourists, hikers, skaters, and beach pedestrians", _allMenus);
            foreach (var child in new[] { propConstruction, propFurniture, propContainers, propManagement, pedCivilians, pedServices, pedActivities })
                MenuNavigation.AddBack(child, child == propConstruction || child == propFurniture || child == propContainers || child == propManagement ? _props : _peds, _allMenus);

            var search = new NativeItem("Search Entity Catalog", "Search prop and pedestrian names or model identifiers");
            search.Activated += (s, e) => SearchCatalog();
            Menu.Add(search);
            AddSubmenu("Prop Spawner", "Browse props by category", _props);
            AddSubmenu("Pedestrian Spawner", "Browse pedestrian models by category", _peds);
            AddSubmenu("Ped Management", "Control tasks and health for trainer-spawned pedestrians", _pedManagement);
            AddSubmenu("Object Placement", "Move, rotate, duplicate, and delete spawned props", _placementMenu.Menu);
            AddBack(Menu, "Back", _goBack);
            MenuNavigation.RegisterParent(_pedManagement, Menu); MenuNavigation.RegisterParent(_searchResults, Menu); MenuNavigation.RegisterParent(_placementMenu.Menu, Menu);

            AddProp(propConstruction, "Barrel", "prop_barrel_02a");
            AddProp(propContainers, "Wooden Crate", "prop_box_wood02a_pu");
            AddProp(propConstruction, "Road Cone", "prop_roadcone02a");
            AddProp(propConstruction, "Barrier", "prop_barrier_work05");
            AddProp(propFurniture, "Bench", "prop_bench_01a");
            AddProp(propFurniture, "Camp Chair", "prop_skid_chair_02");
            AddProp(propFurniture, "Beach Umbrella", "prop_parasol_04e");
            AddProp(propFurniture, "Small Table", "prop_table_03");
            AddProp(propContainers, "Briefcase", "prop_ld_case_01");
            AddProp(propContainers, "Pallet", "prop_pallet_02a");
            AddProp(propManagement, "Delete Last Spawned Prop", null);
            var clearProps = new NativeItem("Delete All Trainer Props", "Remove all props spawned by Titan Trainer in this session");
            clearProps.Activated += (s, e) => _spawner.DeleteAllSpawnedProps();
            propManagement.Add(clearProps);
            AddBack(_props, "Back to Entity Spawner", () => { HideAll(); Show(); });

            AddPed(pedActivities, "Beach Male", "a_m_y_beach_01");
            AddPed(pedCivilians, "Business Male", "a_m_y_business_01");
            AddPed(pedCivilians, "Business Female", "a_f_y_business_01");
            AddPed(pedActivities, "Tourist Male", "a_m_y_tourist_01");
            AddPed(pedActivities, "Hiker", "a_m_y_hiker_01");
            AddPed(pedActivities, "Skater", "a_m_y_skater_01");
            AddPed(pedServices, "Police Officer", "s_m_y_cop_01");
            AddPed(pedServices, "Paramedic", "s_m_m_paramedic_01");
            AddPed(pedServices, "Firefighter", "s_m_y_fireman_01");
            AddPed(pedServices, "Mechanic", "s_m_y_xmech_02");
            AddPed(pedServices, "Delete Last Spawned Pedestrian", null);
            var clearPeds = new NativeItem("Delete All Trainer Pedestrians", "Remove all pedestrians spawned by Titan Trainer in this session");
            clearPeds.Activated += (s, e) => _spawner.DeleteAllSpawnedPeds();
            pedServices.Add(clearPeds);
            AddBack(_peds, "Back to Entity Spawner", () => { HideAll(); Show(); });

            AddPedManagementAction(lastPedManagement, "Follow Player", "Make the last spawned pedestrian follow you", _spawner.MakeLastPedFollowPlayer);
            AddPedManagementAction(lastPedManagement, "Wander Around", "Let the last spawned pedestrian wander", _spawner.MakeLastPedWander);
            AddPedManagementAction(lastPedManagement, "Stop / Clear Tasks", "Clear the last spawned pedestrian's current task", _spawner.StopLastPed);
            AddPedManagementAction(lastPedManagement, "Fight Player", "Order the last spawned pedestrian to fight the player", _spawner.MakeLastPedFightPlayer);
            AddPedManagementAction(lastPedManagement, "Flee from Player", "Order the last spawned pedestrian to flee for 30 seconds", _spawner.MakeLastPedFleePlayer);
            AddPedManagementAction(lastPedManagement, "Heal + 100 Armor", "Restore health and set armor for the last spawned pedestrian", _spawner.HealLastPed);
            AddPedManagementAction(lastPedManagement, "Toggle Invincibility", "Toggle invincibility for the last spawned pedestrian", _spawner.ToggleLastPedInvincibility);
            AddPedManagementAction(allPedManagement, "Make All Follow Player", "Order all trainer-spawned pedestrians to follow you", _spawner.MakeAllPedsFollowPlayer);
            AddPedManagementAction(allPedManagement, "Make All Wander", "Order all trainer-spawned pedestrians to wander", _spawner.MakeAllPedsWander);
            AddPedManagementAction(allPedManagement, "Stop All / Clear Tasks", "Clear tasks for all trainer-spawned pedestrians", _spawner.StopAllPeds);
            AddPedManagementAction(allPedManagement, "Delete All Trainer Pedestrians", "Delete only pedestrians spawned by Titan Trainer", _spawner.DeleteAllSpawnedPeds);
            AddBack(_pedManagement, "Back to Entity Spawner", () => { HideAll(); Show(); });
        }

        private void AddSubmenu(string label, string description, NativeMenu target)
        {
            MenuNavigation.RegisterParent(target, Menu);
            var item = new NativeItem(label, description);
            item.Activated += (s, e) => { HideAll(); target.Visible = true; };
            Menu.Add(item);
        }

        private void AddProp(NativeMenu target, string label, string model)
        {
            var item = new NativeItem(label, model == null ? "Remove the last prop spawned by Titan Trainer" : "Spawn " + model);
            item.Activated += (s, e) =>
            {
                if (model == null) _spawner.DeleteLastProp();
                else _spawner.SpawnProp(model, label);
            };
            target.Add(item);
        }

        private void AddPed(NativeMenu target, string label, string model)
        {
            var item = new NativeItem(label, model == null ? "Remove the last pedestrian spawned by Titan Trainer" : "Spawn " + model);
            item.Activated += (s, e) =>
            {
                if (model == null) _spawner.DeleteLastPed();
                else _spawner.SpawnPed(model, label);
            };
            target.Add(item);
        }

        private void AddPedManagementAction(NativeMenu target, string label, string description, Action action)
        {
            var item = new NativeItem(label, description);
            item.Activated += (s, e) => action?.Invoke();
            target.Add(item);
        }

        private void AddBack(NativeMenu menu, string label, Action action)
        {
            var back = new NativeItem(label, "Return to the previous menu");
            back.Activated += (s, e) => { HideAll(); action?.Invoke(); };
            menu.Add(back);
        }

        private void SearchCatalog()
        {
            string query;
            try
            {
                query = Game.GetUserInput(WindowTitle.EnterMessage60, string.Empty, 32);
            }
            catch (Exception)
            {
                // If the game input prompt is unavailable, leave the current menu unchanged.
                return;
            }

            if (string.IsNullOrWhiteSpace(query)) return;
            query = query.Trim();
            _searchResults.Clear();

            var entries = new List<EntitySearchEntry>
            {
                new EntitySearchEntry("Barrel", "prop_barrel_02a", false),
                new EntitySearchEntry("Wooden Crate", "prop_box_wood02a_pu", false),
                new EntitySearchEntry("Road Cone", "prop_roadcone02a", false),
                new EntitySearchEntry("Barrier", "prop_barrier_work05", false),
                new EntitySearchEntry("Bench", "prop_bench_01a", false),
                new EntitySearchEntry("Camp Chair", "prop_skid_chair_02", false),
                new EntitySearchEntry("Beach Umbrella", "prop_parasol_04e", false),
                new EntitySearchEntry("Small Table", "prop_table_03", false),
                new EntitySearchEntry("Briefcase", "prop_ld_case_01", false),
                new EntitySearchEntry("Pallet", "prop_pallet_02a", false),
                new EntitySearchEntry("Beach Male", "a_m_y_beach_01", true),
                new EntitySearchEntry("Business Male", "a_m_y_business_01", true),
                new EntitySearchEntry("Business Female", "a_f_y_business_01", true),
                new EntitySearchEntry("Tourist Male", "a_m_y_tourist_01", true),
                new EntitySearchEntry("Hiker", "a_m_y_hiker_01", true),
                new EntitySearchEntry("Skater", "a_m_y_skater_01", true),
                new EntitySearchEntry("Police Officer", "s_m_y_cop_01", true),
                new EntitySearchEntry("Paramedic", "s_m_m_paramedic_01", true),
                new EntitySearchEntry("Firefighter", "s_m_y_fireman_01", true),
                new EntitySearchEntry("Mechanic", "s_m_m_autoshop_02", true)
            };

            var matches = entries.Where(x => x.Label.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                             x.Model.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                                 .Take(60).ToList();
            if (matches.Count == 0)
            {
                var empty = new NativeItem("No matching entities", "Try a prop name, ped name, or model identifier");
                empty.Enabled = false;
                _searchResults.Add(empty);
            }
            else
            {
                _searchResults.Add(new NativeItem("Search: " + query, matches.Count + " result(s)") { Enabled = false });
                foreach (var match in matches)
                {
                    var captured = match;
                    var item = new NativeItem(captured.Label, (captured.IsPed ? "Pedestrian · " : "Prop · ") + captured.Model);
                    item.Activated += (s, e) =>
                    {
                        if (captured.IsPed) _spawner.SpawnPed(captured.Model, captured.Label);
                        else _spawner.SpawnProp(captured.Model, captured.Label);
                    };
                    _searchResults.Add(item);
                }
            }

            var searchAgain = new NativeItem("Search Again", "Enter another query");
            searchAgain.Activated += (s, e) => SearchCatalog();
            _searchResults.Add(searchAgain);
            var back = new NativeItem("Back to Entity Spawner", "Return to the previous menu");
            back.Activated += (s, e) => { HideAll(); Show(); };
            _searchResults.Add(back);
            HideAll();
            _searchResults.Visible = true;
        }

        private sealed class EntitySearchEntry
        {
            public string Label { get; }
            public string Model { get; }
            public bool IsPed { get; }

            public EntitySearchEntry(string label, string model, bool isPed)
            {
                Label = label;
                Model = model;
                IsPed = isPed;
            }
        }

        private void HideAll()
        {
            MenuNavigation.Hide(_allMenus);
            _placementMenu.Hide();
        }

        public override void Show() { HideAll(); Menu.Visible = true; }
        public override void Hide() { HideAll(); }
    }
}
