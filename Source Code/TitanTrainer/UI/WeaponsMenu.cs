using System;
using GTA;
using GTA.Native;
using LemonUI.Menus;
using TitanTrainer.Utils;

namespace TitanTrainer.UI
{
    /// <summary>Weapon loadout tools for single-player Story Mode.</summary>
    public sealed class WeaponsMenu : BaseMenu
    {
        private readonly NotificationService _notifications;
        private readonly Action _goBack;

        private static readonly WeaponEntry[] Weapons =
        {
            new WeaponEntry("Unarmed", null, 0),
            new WeaponEntry("Pistol", "WEAPON_PISTOL", 250),
            new WeaponEntry("Combat Pistol", "WEAPON_COMBATPISTOL", 250),
            new WeaponEntry("AP Pistol", "WEAPON_APPISTOL", 250),
            new WeaponEntry("Heavy Pistol", "WEAPON_HEAVYPISTOL", 250),
            new WeaponEntry("Micro SMG", "WEAPON_MICROSMG", 500),
            new WeaponEntry("SMG", "WEAPON_SMG", 500),
            new WeaponEntry("Assault Rifle", "WEAPON_ASSAULTRIFLE", 500),
            new WeaponEntry("Carbine Rifle", "WEAPON_CARBINERIFLE", 500),
            new WeaponEntry("Advanced Rifle", "WEAPON_ADVANCEDRIFLE", 500),
            new WeaponEntry("Pump Shotgun", "WEAPON_PUMPSHOTGUN", 150),
            new WeaponEntry("Sawed-Off Shotgun", "WEAPON_SAWNOFFSHOTGUN", 150),
            new WeaponEntry("Bullpup Shotgun", "WEAPON_BULLPUPSHOTGUN", 150),
            new WeaponEntry("Sniper Rifle", "WEAPON_SNIPERRIFLE", 100),
            new WeaponEntry("Heavy Sniper", "WEAPON_HEAVYSNIPER", 100),
            new WeaponEntry("Grenade Launcher", "WEAPON_GRENADELAUNCHER", 50),
            new WeaponEntry("RPG", "WEAPON_RPG", 50),
            new WeaponEntry("Minigun", "WEAPON_MINIGUN", 1000),
            new WeaponEntry("Grenade", "WEAPON_GRENADE", 25),
            new WeaponEntry("Sticky Bomb", "WEAPON_STICKYBOMB", 25),
            new WeaponEntry("Knife", "WEAPON_KNIFE", 1),
            new WeaponEntry("Bat", "WEAPON_BAT", 1),
            new WeaponEntry("Crowbar", "WEAPON_CROWBAR", 1)
        };

        public WeaponsMenu(NotificationService notifications, Action goBack)
            : base("Titan Trainer", "Weapons & Loadouts")
        {
            _notifications = notifications;
            _goBack = goBack;

            foreach (var entry in Weapons)
            {
                var captured = entry;
                var item = new NativeItem(captured.Name, captured.WeaponName == null ? "Select unarmed" : "Equip with ammunition");
                item.Activated += (s, e) => GiveWeapon(captured);
                Menu.Add(item);
            }

            Menu.Add(new NativeSeparatorItem("Loadout"));
            var full = new NativeItem("Give Standard Arsenal", "Pistols, SMGs, rifles, shotgun, sniper and explosives");
            full.Activated += (s, e) =>
            {
                foreach (var entry in Weapons)
                    if (!string.IsNullOrEmpty(entry.WeaponName)) GiveWeapon(entry, false);
                _notifications?.NotifySuccess("Standard arsenal granted");
            };
            Menu.Add(full);

            var ammo = new NativeItem("Refill All Ammo", "Refill ammunition for carried weapons");
            ammo.Activated += (s, e) =>
            {
                var ped = Game.Player.Character;
                if (ped == null || !ped.Exists()) return;
                Function.Call(Hash.SET_AMMO_IN_CLIP, ped.Handle, Function.Call<int>(Hash.GET_SELECTED_PED_WEAPON, ped.Handle), 999);
                Function.Call(Hash.ADD_AMMO_TO_PED, ped.Handle, Function.Call<int>(Hash.GET_SELECTED_PED_WEAPON, ped.Handle), 9999);
                _notifications?.NotifySuccess("Current weapon ammo refilled");
            };
            Menu.Add(ammo);

            var infinite = new NativeCheckboxItem("Infinite Ammo Clip", false);
            infinite.CheckboxChanged += (s, e) =>
            {
                var ped = Game.Player.Character;
                if (ped != null && ped.Exists()) Function.Call(Hash.SET_PED_INFINITE_AMMO_CLIP, ped.Handle, infinite.Checked);
                _notifications?.NotifyInfo(infinite.Checked ? "Infinite ammo clip enabled" : "Infinite ammo clip disabled");
            };
            Menu.Add(infinite);

            var remove = new NativeItem("Remove All Weapons", "Remove every weapon from the player");
            remove.Activated += (s, e) =>
            {
                var ped = Game.Player.Character;
                if (ped != null && ped.Exists())
                {
                    Function.Call(Hash.REMOVE_ALL_PED_WEAPONS, ped.Handle, true);
                    _notifications?.NotifySuccess("Weapons removed");
                }
            };
            Menu.Add(remove);

            Menu.Add(new NativeSeparatorItem("Navigation"));
            var back = new NativeItem("Back", "Return to the main menu");
            back.Activated += (s, e) => { Hide(); _goBack?.Invoke(); };
            Menu.Add(back);
        }

        private void GiveWeapon(WeaponEntry entry, bool notify = true)
        {
            var ped = Game.Player.Character;
            if (ped == null || !ped.Exists())
            {
                _notifications?.NotifyError("Player character is unavailable");
                return;
            }

            if (string.IsNullOrEmpty(entry.WeaponName))
            {
                Function.Call(Hash.SET_CURRENT_PED_WEAPON, ped.Handle, Function.Call<int>(Hash.GET_HASH_KEY, "WEAPON_UNARMED"), true);
                if (notify) _notifications?.NotifyInfo("Unarmed selected");
                return;
            }

            int hash = Function.Call<int>(Hash.GET_HASH_KEY, entry.WeaponName);
            Function.Call(Hash.GIVE_WEAPON_TO_PED, ped.Handle, hash, entry.Ammo, false, true);
            Function.Call(Hash.SET_PED_AMMO, ped.Handle, hash, entry.Ammo);
            if (notify) _notifications?.NotifySuccess(entry.Name + " equipped");
        }

        private sealed class WeaponEntry
        {
            public string Name { get; }
            public string WeaponName { get; }
            public int Ammo { get; }
            public WeaponEntry(string name, string weaponName, int ammo) { Name = name; WeaponName = weaponName; Ammo = ammo; }
        }
    }
}
