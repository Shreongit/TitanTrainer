using LemonUI.Menus;
using TitanTrainer.Player;
using TitanTrainer.Utils;

namespace TitanTrainer.UI
{
    public sealed class PlayerMenu : BaseMenu
    {
        private readonly PlayerManager _player;
        private readonly System.Collections.Generic.List<NativeMenu> _allMenus = new System.Collections.Generic.List<NativeMenu>();
        private NativeMenu _healthMenu, _movementMenu, _wantedMenu, _teleportMenu;
        public NativeMenu[] AllMenus => _allMenus.ToArray();

        public PlayerMenu(ServiceContainer services)
            : base("Titan Trainer", "Player")
        {
            _player = services.Resolve<PlayerManager>();
            _allMenus.Add(Menu);
            _healthMenu = MenuNavigation.CreateChild(Menu, "Health & Armor", "Health, armor, and survival options", _allMenus);
            _movementMenu = MenuNavigation.CreateChild(Menu, "Movement & Abilities", "Movement, visibility, and ability toggles", _allMenus);
            _wantedMenu = MenuNavigation.CreateChild(Menu, "Wanted Level", "Police attention and wanted-level controls", _allMenus);
            _teleportMenu = MenuNavigation.CreateChild(Menu, "Player Teleport", "Quick player movement and waypoint travel", _allMenus);
            MenuNavigation.AddBack(_healthMenu, Menu, _allMenus);
            MenuNavigation.AddBack(_movementMenu, Menu, _allMenus);
            MenuNavigation.AddBack(_wantedMenu, Menu, _allMenus);
            MenuNavigation.AddBack(_teleportMenu, Menu, _allMenus);

            //---------------------------------
            // Health
            //---------------------------------

            var heal = new NativeItem(
                "Heal Player",
                "Restore full health"
            );

            heal.Activated += (s, e) =>
            {
                _player.Health.Heal();
            };

            _healthMenu.Add(heal);

            var armor = new NativeItem(
                "Max Armor",
                "Restore armor"
            );

            armor.Activated += (s, e) =>
            {
                _player.Health.MaxArmor();
            };

            _healthMenu.Add(armor);

            var revive = new NativeItem(
                "Revive",
                "Revive player"
            );

            revive.Activated += (s, e) =>
            {
                _player.Health.Revive();
            };

            _healthMenu.Add(revive);

            

            var infiniteHealth = new NativeCheckboxItem(
                "Infinite Health",
                false
            );

            infiniteHealth.CheckboxChanged += (s, e) =>
            {
                _player.Health.InfiniteHealth =
                    infiniteHealth.Checked;
            };

            _healthMenu.Add(infiniteHealth);

            var infiniteArmor = new NativeCheckboxItem(
                "Infinite Armor",
                false
            );

            infiniteArmor.CheckboxChanged += (s, e) =>
            {
                _player.Health.InfiniteArmor =
                    infiniteArmor.Checked;
            };

            _healthMenu.Add(infiniteArmor);

            

            var superJump = new NativeCheckboxItem(
                "Super Jump",
                false
            );

            superJump.CheckboxChanged += (s, e) =>
            {
                _player.Movement.SuperJump =
                    superJump.Checked;
            };

            _movementMenu.Add(superJump);

            var fastRun = new NativeCheckboxItem(
                "Fast Run",
                false
            );

            fastRun.CheckboxChanged += (s, e) =>
            {
                _player.Movement.FastRun =
                    fastRun.Checked;
            };

            _movementMenu.Add(fastRun);

            var stamina = new NativeCheckboxItem(
                "Infinite Stamina",
                false
            );

            stamina.CheckboxChanged += (s, e) =>
            {
                _player.Movement.InfiniteStamina =
                    stamina.Checked;
            };

            _movementMenu.Add(stamina);

            var ragdoll = new NativeCheckboxItem(
                "No Ragdoll",
                false
            );

            ragdoll.CheckboxChanged += (s, e) =>
            {
                _player.Movement.NoRagdoll =
                    ragdoll.Checked;
            };

            _movementMenu.Add(ragdoll);

            var invisible = new NativeCheckboxItem(
                "Invisible",
                false
            );

            invisible.CheckboxChanged += (s, e) =>
            {
                _player.Movement.Invisible =
                    invisible.Checked;
            };

            _movementMenu.Add(invisible);

            

            var neverWanted = new NativeCheckboxItem(
                "Never Wanted",
                false
            );

            neverWanted.CheckboxChanged += (s, e) =>
            {
                _player.Wanted.NeverWanted =
                    neverWanted.Checked;
            };

            _wantedMenu.Add(neverWanted);

            var freezeWanted = new NativeCheckboxItem(
                "Freeze Wanted Level",
                false
            );

            freezeWanted.CheckboxChanged += (s, e) =>
            {
                _player.Wanted.FreezeWanted =
                    freezeWanted.Checked;
            };

            _wantedMenu.Add(freezeWanted);

            var clearWanted = new NativeItem(
                "Clear Wanted Level"
            );

            clearWanted.Activated += (s, e) =>
            {
                _player.Wanted.ClearWanted();
            };

            _wantedMenu.Add(clearWanted);

            var wanted5 = new NativeItem(
                "Set Wanted Level ★★★★★"
            );

            wanted5.Activated += (s, e) =>
            {
                _player.Wanted.SetWantedLevel(5);
            };

            _wantedMenu.Add(wanted5);

            

            var waypoint = new NativeItem(
                "Teleport To Waypoint"
            );

            waypoint.Activated += (s, e) =>
            {
                _player.Teleport.TeleportToWaypoint();
            };

            _teleportMenu.Add(waypoint);

            var forward = new NativeItem(
                "Teleport Forward"
            );

            forward.Activated += (s, e) =>
            {
                _player.Teleport.TeleportForward();
            };

            _teleportMenu.Add(forward);

            var safe = new NativeItem(
                "Teleport To Ground"
            );

            safe.Activated += (s, e) =>
            {
                _player.Teleport.TeleportSafe();
            };

            _teleportMenu.Add(safe);

            var sky = new NativeItem(
                "Teleport To Sky"
            );

            sky.Activated += (s, e) =>
            {
                _player.Teleport.TeleportSky();
            };

            _teleportMenu.Add(sky);
        }
        public override void Hide() => MenuNavigation.Hide(_allMenus);
        public override void Show() { MenuNavigation.Hide(_allMenus); Menu.Visible = true; }
    }
}