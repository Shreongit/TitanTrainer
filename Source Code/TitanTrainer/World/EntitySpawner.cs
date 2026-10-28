using System;
using GTA;
using GTA.Math;
using TitanTrainer.Utils;

namespace TitanTrainer.World
{
    /// <summary>Safe, curated prop and pedestrian spawning for GTA V Story Mode.</summary>
    public sealed class EntitySpawner : IDisposable
    {
        private readonly NotificationService _notifications;
        private Entity _lastProp;
        private Ped _lastPed;
        private bool _disposed;

        public EntitySpawner(NotificationService notifications)
        {
            _notifications = notifications;
        }

        public void SpawnProp(string modelName, string displayName)
        {
            if (_disposed) return;
            try
            {
                var player = Game.Player.Character;
                if (player == null || !player.Exists())
                {
                    _notifications?.NotifyError("Player character is unavailable");
                    return;
                }

                Model model = new Model(modelName);
                if (!model.IsInCdImage || !model.IsValid)
                {
                    _notifications?.NotifyError("Invalid prop model: " + modelName);
                    return;
                }

                model.Request(1000);
                if (!model.IsLoaded)
                {
                    _notifications?.NotifyError("Prop model failed to load: " + modelName);
                    model.MarkAsNoLongerNeeded();
                    return;
                }

                Vector3 position = player.Position + player.ForwardVector * 2.5f;
                Prop prop = GTA.World.CreateProp(model, position, true, true);
                model.MarkAsNoLongerNeeded();
                if (prop == null || !prop.Exists())
                {
                    _notifications?.NotifyError("Could not spawn " + displayName);
                    return;
                }

                _lastProp = prop;
                _notifications?.NotifySuccess(displayName + " spawned");
            }
            catch (Exception ex)
            {
                Logger.Warn("Prop spawn failed for " + modelName + ": " + ex.Message);
                _notifications?.NotifyError("Prop spawn failed");
            }
        }

        public void SpawnPed(string modelName, string displayName)
        {
            if (_disposed) return;
            try
            {
                var player = Game.Player.Character;
                if (player == null || !player.Exists())
                {
                    _notifications?.NotifyError("Player character is unavailable");
                    return;
                }

                Model model = new Model(modelName);
                if (!model.IsInCdImage || !model.IsValid || !model.IsPed)
                {
                    _notifications?.NotifyError("Invalid pedestrian model: " + modelName);
                    return;
                }

                model.Request(1000);
                if (!model.IsLoaded)
                {
                    _notifications?.NotifyError("Pedestrian model failed to load: " + modelName);
                    model.MarkAsNoLongerNeeded();
                    return;
                }

                Vector3 position = player.Position + player.ForwardVector * 3.0f;
                Ped ped = GTA.World.CreatePed(model, position);
                model.MarkAsNoLongerNeeded();
                if (ped == null || !ped.Exists())
                {
                    _notifications?.NotifyError("Could not spawn " + displayName);
                    return;
                }

                ped.IsPersistent = true;
                ped.BlockPermanentEvents = true;
                _lastPed = ped;
                _notifications?.NotifySuccess(displayName + " spawned");
            }
            catch (Exception ex)
            {
                Logger.Warn("Ped spawn failed for " + modelName + ": " + ex.Message);
                _notifications?.NotifyError("Pedestrian spawn failed");
            }
        }

        public void DeleteLastProp()
        {
            DeleteEntity(_lastProp, "Last spawned prop removed");
            _lastProp = null;
        }

        public void DeleteLastPed()
        {
            DeleteEntity(_lastPed, "Last spawned pedestrian removed");
            _lastPed = null;
        }

        private void DeleteEntity(Entity entity, string message)
        {
            try
            {
                if (entity != null && entity.Exists())
                {
                    entity.Delete();
                    _notifications?.NotifySuccess(message);
                }
                else
                {
                    _notifications?.NotifyInfo("No matching spawned entity to remove");
                }
            }
            catch (Exception ex)
            {
                Logger.Warn("Entity cleanup failed: " + ex.Message);
                _notifications?.NotifyError("Could not remove entity");
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            // Do not delete entities automatically on script reload; player-created world state is preserved.
            _lastProp = null;
            _lastPed = null;
        }
    }
}
