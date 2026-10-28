using System;
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
        private readonly Action _goBack;

        public NativeMenu[] AllMenus { get; }

        public EntitySpawnerMenu(EntitySpawner spawner, Action goBack)
            : base("Titan Trainer", "Entity Spawner")
        {
            _spawner = spawner;
            _goBack = goBack;
            _props = new NativeMenu("Titan Trainer", "Prop Spawner");
            _peds = new NativeMenu("Titan Trainer", "Pedestrian Spawner");
            _pedManagement = new NativeMenu("Titan Trainer", "Ped Management");
            _placementMenu = new ObjectPlacementMenu(spawner, () => { HideAll(); Show(); });
            _props.Visible = false;
            _peds.Visible = false;
            AllMenus = new[] { Menu, _props, _peds, _pedManagement, _placementMenu.Menu };

            AddSubmenu("Prop Spawner", "Spawn useful props in front of the player", _props);
            AddSubmenu("Pedestrian Spawner", "Spawn selected pedestrians in front of the player", _peds);
            AddSubmenu("Ped Management", "Control tasks and health for trainer-spawned pedestrians", _pedManagement);
            AddSubmenu("Object Placement", "Move, rotate, duplicate, and delete spawned props", _placementMenu.Menu);
            AddBack(Menu, "Back", _goBack);

            AddProp("Barrel", "prop_barrel_02a");
            AddProp("Wooden Crate", "prop_box_wood02a_pu");
            AddProp("Road Cone", "prop_roadcone02a");
            AddProp("Barrier", "prop_barrier_work05");
            AddProp("Bench", "prop_bench_01a");
            AddProp("Camp Chair", "prop_skid_chair_02");
            AddProp("Beach Umbrella", "prop_parasol_04e");
            AddProp("Small Table", "prop_table_03");
            AddProp("Briefcase", "prop_ld_case_01");
            AddProp("Pallet", "prop_pallet_02a");
            AddProp("Delete Last Spawned Prop", null);
            var clearProps = new NativeItem("Delete All Trainer Props", "Remove all props spawned by Titan Trainer in this session");
            clearProps.Activated += (s, e) => _spawner.DeleteAllSpawnedProps();
            _props.Add(clearProps);
            AddBack(_props, "Back to Entity Spawner", () => { HideAll(); Show(); });

            AddPed("Beach Male", "a_m_y_beach_01");
            AddPed("Business Male", "a_m_y_business_01");
            AddPed("Business Female", "a_f_y_business_01");
            AddPed("Tourist Male", "a_m_y_tourist_01");
            AddPed("Hiker", "a_m_y_hiker_01");
            AddPed("Skater", "a_m_y_skater_01");
            AddPed("Police Officer", "s_m_y_cop_01");
            AddPed("Paramedic", "s_m_m_paramedic_01");
            AddPed("Firefighter", "s_m_y_fireman_01");
            AddPed("Mechanic", "s_m_y_xmech_02");
            AddPed("Delete Last Spawned Pedestrian", null);
            var clearPeds = new NativeItem("Delete All Trainer Pedestrians", "Remove all pedestrians spawned by Titan Trainer in this session");
            clearPeds.Activated += (s, e) => _spawner.DeleteAllSpawnedPeds();
            _peds.Add(clearPeds);
            AddBack(_peds, "Back to Entity Spawner", () => { HideAll(); Show(); });

            _pedManagement.Add(new NativeSeparatorItem("Last Spawned Pedestrian"));
            AddPedManagementAction("Follow Player", "Make the last spawned pedestrian follow you", _spawner.MakeLastPedFollowPlayer);
            AddPedManagementAction("Wander Around", "Let the last spawned pedestrian wander", _spawner.MakeLastPedWander);
            AddPedManagementAction("Stop / Clear Tasks", "Clear the last spawned pedestrian's current task", _spawner.StopLastPed);
            AddPedManagementAction("Fight Player", "Order the last spawned pedestrian to fight the player", _spawner.MakeLastPedFightPlayer);
            AddPedManagementAction("Flee from Player", "Order the last spawned pedestrian to flee for 30 seconds", _spawner.MakeLastPedFleePlayer);
            AddPedManagementAction("Heal + 100 Armor", "Restore health and set armor for the last spawned pedestrian", _spawner.HealLastPed);
            AddPedManagementAction("Toggle Invincibility", "Toggle invincibility for the last spawned pedestrian", _spawner.ToggleLastPedInvincibility);
            _pedManagement.Add(new NativeSeparatorItem("All Trainer Pedestrians"));
            AddPedManagementAction("Make All Follow Player", "Order all trainer-spawned pedestrians to follow you", _spawner.MakeAllPedsFollowPlayer);
            AddPedManagementAction("Make All Wander", "Order all trainer-spawned pedestrians to wander", _spawner.MakeAllPedsWander);
            AddPedManagementAction("Stop All / Clear Tasks", "Clear tasks for all trainer-spawned pedestrians", _spawner.StopAllPeds);
            AddPedManagementAction("Delete All Trainer Pedestrians", "Delete only pedestrians spawned by Titan Trainer", _spawner.DeleteAllSpawnedPeds);
            AddBack(_pedManagement, "Back to Entity Spawner", () => { HideAll(); Show(); });
        }

        private void AddSubmenu(string label, string description, NativeMenu target)
        {
            var item = new NativeItem(label, description);
            item.Activated += (s, e) => { HideAll(); target.Visible = true; };
            Menu.Add(item);
        }

        private void AddProp(string label, string model)
        {
            var item = new NativeItem(label, model == null ? "Remove the last prop spawned by Titan Trainer" : "Spawn " + model);
            item.Activated += (s, e) =>
            {
                if (model == null) _spawner.DeleteLastProp();
                else _spawner.SpawnProp(model, label);
            };
            _props.Add(item);
        }

        private void AddPed(string label, string model)
        {
            var item = new NativeItem(label, model == null ? "Remove the last pedestrian spawned by Titan Trainer" : "Spawn " + model);
            item.Activated += (s, e) =>
            {
                if (model == null) _spawner.DeleteLastPed();
                else _spawner.SpawnPed(model, label);
            };
            _peds.Add(item);
        }

        private void AddPedManagementAction(string label, string description, Action action)
        {
            var item = new NativeItem(label, description);
            item.Activated += (s, e) => action?.Invoke();
            _pedManagement.Add(item);
        }

        private void AddBack(NativeMenu menu, string label, Action action)
        {
            menu.Add(new NativeSeparatorItem("Navigation"));
            var back = new NativeItem(label, "Return to the previous menu");
            back.Activated += (s, e) => { HideAll(); action?.Invoke(); };
            menu.Add(back);
        }

        private void HideAll()
        {
            Menu.Visible = false;
            _props.Visible = false;
            _peds.Visible = false;
            _pedManagement.Visible = false;
            _placementMenu.Hide();
        }

        public override void Show() { HideAll(); Menu.Visible = true; }
        public override void Hide() { HideAll(); }
    }
}
