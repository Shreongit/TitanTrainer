using System;
using TitanTrainer.Utils;

namespace TitanTrainer.Player
{
    /// <summary>
    /// Central player coordinator.
    /// </summary>
    public sealed class PlayerManager : IDisposable
    {
        public PlayerHealth Health { get; }

        public PlayerMovement Movement { get; }

        public PlayerWanted Wanted { get; }

        public TeleportManager Teleport { get; }

        public PlayerManager(NotificationService notifications)
        {
            Health = new PlayerHealth(notifications);

            Movement = new PlayerMovement(notifications);

            Wanted = new PlayerWanted(notifications);

            Teleport = new TeleportManager(notifications);
        }

        public void Update()
        {
            Health.Update();

            Movement.Update();

            Wanted.Update();
        }

        public void Dispose()
        {
        }
    }
}