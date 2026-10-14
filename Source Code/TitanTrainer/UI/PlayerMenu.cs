using LemonUI.Menus;
using TitanTrainer.Player;
using TitanTrainer.Utils;

namespace TitanTrainer.UI
{
    public sealed class PlayerMenu : BaseMenu
    {
        private readonly PlayerManager _player;

        public PlayerMenu(ServiceContainer services)
            : base("Titan Trainer", "Player")
        {
            _player = services.Resolve<PlayerManager>();

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

            Menu.Add(heal);

            var armor = new NativeItem(
                "Max Armor",
                "Restore armor"
            );

            armor.Activated += (s, e) =>
            {
                _player.Health.MaxArmor();
            };

            Menu.Add(armor);

            var revive = new NativeItem(
                "Revive",
                "Revive player"
            );

            revive.Activated += (s, e) =>
            {
                _player.Health.Revive();
            };

            Menu.Add(revive);

            Menu.Add(new NativeSeparatorItem("Health"));

            var infiniteHealth = new NativeCheckboxItem(
                "Infinite Health",
                false
            );

            infiniteHealth.CheckboxChanged += (s, e) =>
            {
                _player.Health.InfiniteHealth =
                    infiniteHealth.Checked;
            };

            Menu.Add(infiniteHealth);

            var infiniteArmor = new NativeCheckboxItem(
                "Infinite Armor",
                false
            );

            infiniteArmor.CheckboxChanged += (s, e) =>
            {
                _player.Health.InfiniteArmor =
                    infiniteArmor.Checked;
            };

            Menu.Add(infiniteArmor);

            Menu.Add(new NativeSeparatorItem("Movement"));

            var superJump = new NativeCheckboxItem(
                "Super Jump",
                false
            );

            superJump.CheckboxChanged += (s, e) =>
            {
                _player.Movement.SuperJump =
                    superJump.Checked;
            };

            Menu.Add(superJump);

            var fastRun = new NativeCheckboxItem(
                "Fast Run",
                false
            );

            fastRun.CheckboxChanged += (s, e) =>
            {
                _player.Movement.FastRun =
                    fastRun.Checked;
            };

            Menu.Add(fastRun);

            var stamina = new NativeCheckboxItem(
                "Infinite Stamina",
                false
            );

            stamina.CheckboxChanged += (s, e) =>
            {
                _player.Movement.InfiniteStamina =
                    stamina.Checked;
            };

            Menu.Add(stamina);

            var ragdoll = new NativeCheckboxItem(
                "No Ragdoll",
                false
            );

            ragdoll.CheckboxChanged += (s, e) =>
            {
                _player.Movement.NoRagdoll =
                    ragdoll.Checked;
            };

            Menu.Add(ragdoll);

            var invisible = new NativeCheckboxItem(
                "Invisible",
                false
            );

            invisible.CheckboxChanged += (s, e) =>
            {
                _player.Movement.Invisible =
                    invisible.Checked;
            };

            Menu.Add(invisible);

            Menu.Add(new NativeSeparatorItem("Wanted"));

            var neverWanted = new NativeCheckboxItem(
                "Never Wanted",
                false
            );

            neverWanted.CheckboxChanged += (s, e) =>
            {
                _player.Wanted.NeverWanted =
                    neverWanted.Checked;
            };

            Menu.Add(neverWanted);

            var freezeWanted = new NativeCheckboxItem(
                "Freeze Wanted Level",
                false
            );

            freezeWanted.CheckboxChanged += (s, e) =>
            {
                _player.Wanted.FreezeWanted =
                    freezeWanted.Checked;
            };

            Menu.Add(freezeWanted);

            var clearWanted = new NativeItem(
                "Clear Wanted Level"
            );

            clearWanted.Activated += (s, e) =>
            {
                _player.Wanted.ClearWanted();
            };

            Menu.Add(clearWanted);

            var wanted5 = new NativeItem(
                "Set Wanted Level ★★★★★"
            );

            wanted5.Activated += (s, e) =>
            {
                _player.Wanted.SetWantedLevel(5);
            };

            Menu.Add(wanted5);

            Menu.Add(new NativeSeparatorItem("Teleport"));

            var waypoint = new NativeItem(
                "Teleport To Waypoint"
            );

            waypoint.Activated += (s, e) =>
            {
                _player.Teleport.TeleportToWaypoint();
            };

            Menu.Add(waypoint);

            var forward = new NativeItem(
                "Teleport Forward"
            );

            forward.Activated += (s, e) =>
            {
                _player.Teleport.TeleportForward();
            };

            Menu.Add(forward);

            var safe = new NativeItem(
                "Teleport To Ground"
            );

            safe.Activated += (s, e) =>
            {
                _player.Teleport.TeleportSafe();
            };

            Menu.Add(safe);

            var sky = new NativeItem(
                "Teleport To Sky"
            );

            sky.Activated += (s, e) =>
            {
                _player.Teleport.TeleportSky();
            };

            Menu.Add(sky);
        }
    }
}