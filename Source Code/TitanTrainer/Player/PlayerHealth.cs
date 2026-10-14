using GTA;
using TitanTrainer.Core;
using TitanTrainer.Utils;

namespace TitanTrainer.Player
{
    public sealed class PlayerHealth
    {
        private readonly NotificationService _notifications;

        private bool _infiniteHealth;
        private bool _infiniteArmor;

        public PlayerHealth(NotificationService notifications)
        {
            _notifications = notifications;
        }

        public bool InfiniteHealth
        {
            get => _infiniteHealth;
            set
            {
                _infiniteHealth = value;
                _notifications?.NotifyInfo(
                    value ? "Infinite Health Enabled"
                          : "Infinite Health Disabled");
            }
        }

        public bool InfiniteArmor
        {
            get => _infiniteArmor;
            set
            {
                _infiniteArmor = value;
                _notifications?.NotifyInfo(
                    value ? "Infinite Armor Enabled"
                          : "Infinite Armor Disabled");
            }
        }

        public void Heal()
        {
            var ped = Game.Player.Character;

            ped.Health = ped.MaxHealth;

            _notifications?.NotifySuccess("Health Restored");

            Logger.Info("Player healed.");
        }

        public void MaxArmor()
        {
            Game.Player.Character.Armor = 100;

            _notifications?.NotifySuccess("Armor Restored");

            Logger.Info("Player armor restored.");
        }

        public void Revive()
        {
            var ped = Game.Player.Character;

            if (ped.IsDead)
            {
                ped.Resurrect();
            }

            ped.Health = ped.MaxHealth;
            ped.Armor = 100;

            _notifications?.NotifySuccess("Player Revived");

            Logger.Info("Player revived.");
        }

        public void Update()
        {
            var ped = Game.Player.Character;

            if (_infiniteHealth)
            {
                if (ped.Health < ped.MaxHealth)
                    ped.Health = ped.MaxHealth;
            }

            if (_infiniteArmor)
            {
                if (ped.Armor < 100)
                    ped.Armor = 100;
            }
        }
    }
}