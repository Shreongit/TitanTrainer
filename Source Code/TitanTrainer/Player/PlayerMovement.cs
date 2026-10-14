using GTA;
using GTA.Native;
using TitanTrainer.Core;
using TitanTrainer.Utils;

namespace TitanTrainer.Player
{
    public sealed class PlayerMovement
    {
        private readonly NotificationService _notifications;

        private bool _superJump;
        private bool _fastRun;
        private bool _infiniteStamina;
        private bool _noRagdoll;
        private bool _invisible;

        public PlayerMovement(NotificationService notifications)
        {
            _notifications = notifications;
        }

        public bool SuperJump
        {
            get => _superJump;
            set
            {
                _superJump = value;
                _notifications?.NotifyInfo(value
                    ? "Super Jump Enabled"
                    : "Super Jump Disabled");
            }
        }

        public bool FastRun
        {
            get => _fastRun;
            set
            {
                _fastRun = value;
                _notifications?.NotifyInfo(value
                    ? "Fast Run Enabled"
                    : "Fast Run Disabled");
            }
        }

        public bool InfiniteStamina
        {
            get => _infiniteStamina;
            set
            {
                _infiniteStamina = value;
                _notifications?.NotifyInfo(value
                    ? "Infinite Stamina Enabled"
                    : "Infinite Stamina Disabled");
            }
        }

        public bool NoRagdoll
        {
            get => _noRagdoll;
            set
            {
                _noRagdoll = value;
                _notifications?.NotifyInfo(value
                    ? "No Ragdoll Enabled"
                    : "No Ragdoll Disabled");
            }
        }

        public bool Invisible
        {
            get => _invisible;
            set
            {
                _invisible = value;

                Game.Player.Character.IsVisible = !value;

                _notifications?.NotifyInfo(value
                    ? "Player Invisible"
                    : "Player Visible");
            }
        }

        public void Update()
        {
            Ped ped = Game.Player.Character;

            if (_infiniteStamina)
            {
                // TODO: Implement using native for this SHVDN version.

            }

            if (_fastRun)
            {
                Function.Call(Hash.SET_RUN_SPRINT_MULTIPLIER_FOR_PLAYER,
                    Game.Player.Handle,
                    1.49f);
            }
            else
            {
                Function.Call(Hash.SET_RUN_SPRINT_MULTIPLIER_FOR_PLAYER,
                    Game.Player.Handle,
                    1.0f);
            }

            if (_superJump)
            {
                Function.Call(Hash.SET_SUPER_JUMP_THIS_FRAME,
                    Game.Player.Handle);
            }

            if (_noRagdoll)
            {
                ped.CanRagdoll = false;
            }
            else
            {
                ped.CanRagdoll = true;
            }
        }
    }
}