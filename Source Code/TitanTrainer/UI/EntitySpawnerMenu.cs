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
        private readonly Action _goBack;

        public NativeMenu[] AllMenus { get; }

        public EntitySpawnerMenu(EntitySpawner spawner, Action goBack)
            : base("Titan Trainer", "Entity Spawner")
        {
            _spawner = spawner;
            _goBack = goBack;
            _props = new NativeMenu("Titan Trainer", "Prop Spawner");
            _peds = new NativeMenu("Titan Trainer", "Pedestrian Spawner");
            _props.Visible = false;
            _peds.Visible = false;
            AllMenus = new[] { Menu, _props, _peds };

            AddSubmenu("Prop Spawner", "Spawn useful props in front of the player", _props);
            AddSubmenu("Pedestrian Spawner", "Spawn selected pedestrians in front of the player", _peds);
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
            AddBack(_peds, "Back to Entity Spawner", () => { HideAll(); Show(); });
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
        }

        public override void Show() { HideAll(); Menu.Visible = true; }
        public override void Hide() { HideAll(); }
    }
}
