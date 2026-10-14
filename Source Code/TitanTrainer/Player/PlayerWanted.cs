using GTA;
using TitanTrainer.Core;
using TitanTrainer.Utils;

namespace TitanTrainer.Player
{
    public sealed class PlayerWanted
    {
        private readonly NotificationService _notifications;

        private bool _neverWanted;
        private bool _freezeWanted;

        public PlayerWanted(NotificationService notifications)
        {
            _notifications = notifications;
        }

        public bool NeverWanted
        {
            get => _neverWanted;
            set
            {
                _neverWanted = value;

                _notifications?.NotifyInfo(
                    value
                        ? "Never Wanted Enabled"
                        : "Never Wanted Disabled");
            }
        }

        public bool FreezeWanted
        {
            get => _freezeWanted;
            set
            {
                _freezeWanted = value;

                _notifications?.NotifyInfo(
                    value
                        ? "Freeze Wanted Enabled"
                        : "Freeze Wanted Disabled");
            }
        }

        public void ClearWanted()
        {
            Game.Player.Wanted.SetWantedLevel(0, false);
            Game.Player.Wanted.ApplyWantedLevelChangeNow(false);

            _notifications?.NotifySuccess("Wanted Level Cleared");

            Logger.Info("Wanted level cleared.");
        }
        
        public void SetWantedLevel(int stars)
        {
            if (stars < 0) stars = 0;
            if (stars > 5) stars = 5;

            Game.Player.Wanted.SetWantedLevel(stars, false);
            Game.Player.Wanted.ApplyWantedLevelChangeNow(false);

            _notifications?.NotifySuccess($"Wanted Level: {stars}");

            Logger.Info($"Wanted level set to {stars}.");
        }

        public void Update()
        {
            if (_neverWanted)
            {
                if (Game.Player.Wanted.WantedLevel != 0)
                {
                    Game.Player.Wanted.SetWantedLevel(0, false);
                    Game.Player.Wanted.ApplyWantedLevelChangeNow(false);
                }

                return;
            }

            if (_freezeWanted)
            {
                int level = Game.Player.Wanted.WantedLevel;
                Game.Player.Wanted.SetWantedLevel(level, false);
                Game.Player.Wanted.ApplyWantedLevelChangeNow(false);
            }
        }
    }
}