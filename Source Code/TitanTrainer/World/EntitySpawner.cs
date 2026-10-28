using System;
using System.Collections.Generic;
using GTA;
using GTA.Math;
using TitanTrainer.Utils;

namespace TitanTrainer.World
{
    /// <summary>Safe, curated prop and pedestrian spawning for GTA V Story Mode.</summary>
    public sealed class EntitySpawner : IDisposable
    {
        private readonly NotificationService _notifications;
        private Prop _lastProp;
        private Ped _lastPed;
        private string _lastPropModel;
        private readonly List<Prop> _spawnedProps = new List<Prop>();
        private readonly Dictionary<int, string> _propModels = new Dictionary<int, string>();
        private readonly List<Ped> _spawnedPeds = new List<Ped>();
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
                _lastPropModel = modelName;
                _spawnedProps.Add(prop);
                _propModels[prop.Handle] = modelName;
                PruneMissingEntities();
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
                _spawnedPeds.Add(ped);
                PruneMissingEntities();
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
            _spawnedProps.Remove(_lastProp);
            _lastProp = FindLastExisting(_spawnedProps);
            _lastPropModel = null;
            if (_lastProp != null) _propModels.TryGetValue(_lastProp.Handle, out _lastPropModel);
        }

        public void DeleteLastPed()
        {
            DeleteEntity(_lastPed, "Last spawned pedestrian removed");
            _spawnedPeds.Remove(_lastPed);
            _lastPed = FindLastExisting(_spawnedPeds);
        }


        /// <summary>Moves the last prop relative to the player's current facing direction.</summary>
        public void MoveLastProp(float forward, float right, float up)
        {
            if (!TryGetLastProp(out var prop)) return;
            try
            {
                var player = Game.Player.Character;
                Vector3 delta = Vector3.Zero;
                if (player != null && player.Exists())
                    delta = player.ForwardVector * forward + player.RightVector * right;
                delta.Z += up;
                prop.Position = prop.Position + delta;
                _notifications?.NotifyInfo("Object moved");
            }
            catch (Exception ex) { Logger.Warn("Object move failed: " + ex.Message); _notifications?.NotifyError("Could not move object"); }
        }

        /// <summary>Rotates the last prop around its vertical axis.</summary>
        public void RotateLastProp(float degrees)
        {
            if (!TryGetLastProp(out var prop)) return;
            try { prop.Heading = (prop.Heading + degrees + 360.0f) % 360.0f; _notifications?.NotifyInfo("Object rotated"); }
            catch (Exception ex) { Logger.Warn("Object rotation failed: " + ex.Message); _notifications?.NotifyError("Could not rotate object"); }
        }

        public void ResetLastPropRotation()
        {
            if (!TryGetLastProp(out var prop)) return;
            try { prop.Rotation = Vector3.Zero; _notifications?.NotifyInfo("Object rotation reset"); }
            catch (Exception ex) { Logger.Warn("Object rotation reset failed: " + ex.Message); _notifications?.NotifyError("Could not reset rotation"); }
        }

        public void DuplicateLastProp()
        {
            if (!TryGetLastProp(out var prop)) return;
            if (string.IsNullOrWhiteSpace(_lastPropModel))
            {
                _notifications?.NotifyError("Original object model is unavailable");
                return;
            }
            try
            {
                Model model = new Model(_lastPropModel);
                if (!model.IsInCdImage || !model.IsValid)
                {
                    _notifications?.NotifyError("Object model is no longer valid");
                    return;
                }
                model.Request(1000);
                if (!model.IsLoaded)
                {
                    model.MarkAsNoLongerNeeded();
                    _notifications?.NotifyError("Object model failed to load");
                    return;
                }
                Prop duplicate = GTA.World.CreateProp(model, prop.Position + new Vector3(1.0f, 0.0f, 0.0f), true, true);
                model.MarkAsNoLongerNeeded();
                if (duplicate == null || !duplicate.Exists())
                {
                    _notifications?.NotifyError("Could not duplicate object");
                    return;
                }
                duplicate.Rotation = prop.Rotation;
                duplicate.Heading = prop.Heading;
                _lastProp = duplicate;
                _spawnedProps.Add(duplicate);
                _propModels[duplicate.Handle] = _lastPropModel;
                PruneMissingEntities();
                _notifications?.NotifySuccess("Object duplicated");
            }
            catch (Exception ex) { Logger.Warn("Object duplication failed: " + ex.Message); _notifications?.NotifyError("Could not duplicate object"); }
        }

        public void DeleteAllSpawnedProps()
        {
            int removed = DeleteTracked(_spawnedProps);
            _lastProp = null;
            _lastPropModel = null;
            _propModels.Clear();
            _notifications?.NotifyInfo("Removed " + removed + " trainer-spawned prop(s)");
        }

        public void DeleteAllSpawnedPeds()
        {
            int removed = DeleteTracked(_spawnedPeds);
            _lastPed = null;
            _notifications?.NotifyInfo("Removed " + removed + " trainer-spawned pedestrian(s)");
        }


        /// <summary>Orders the last trainer-spawned pedestrian to follow the player.</summary>
        public void MakeLastPedFollowPlayer()
        {
            ApplyToLastPed("Pedestrian is following you", ped =>
            {
                var player = Game.Player.Character;
                if (player == null || !player.Exists()) throw new InvalidOperationException("Player character is unavailable");
                ped.Task.ClearAll();
                ped.Task.FollowToOffsetOfEntity(player, Vector3.Zero, 2.0f, -1, 3.0f, true);
            });
        }

        /// <summary>Orders all trainer-spawned pedestrians to follow the player.</summary>
        public void MakeAllPedsFollowPlayer()
        {
            ApplyToAllPeds(ped =>
            {
                var player = Game.Player.Character;
                if (player == null || !player.Exists()) throw new InvalidOperationException("Player character is unavailable");
                ped.Task.ClearAll();
                ped.Task.FollowToOffsetOfEntity(player, Vector3.Zero, 2.0f, -1, 3.0f, true);
            }, "Trainer pedestrians are following you");
        }

        public void MakeLastPedWander() => ApplyToLastPed("Pedestrian is wandering", ped => { ped.Task.ClearAll(); ped.Task.WanderAround(); });
        public void MakeAllPedsWander() => ApplyToAllPeds(ped => { ped.Task.ClearAll(); ped.Task.WanderAround(); }, "Trainer pedestrians are wandering");
        public void StopLastPed() => ApplyToLastPed("Pedestrian stopped", ped => ped.Task.ClearAll());
        public void StopAllPeds() => ApplyToAllPeds(ped => ped.Task.ClearAll(), "Trainer pedestrian tasks cleared");

        public void MakeLastPedFightPlayer()
        {
            ApplyToLastPed("Pedestrian ordered to fight you", ped =>
            {
                var player = Game.Player.Character;
                if (player == null || !player.Exists()) throw new InvalidOperationException("Player character is unavailable");
                ped.Task.ClearAll();
                ped.Task.FightAgainst(player);
            });
        }

        public void MakeLastPedFleePlayer()
        {
            ApplyToLastPed("Pedestrian is fleeing", ped =>
            {
                var player = Game.Player.Character;
                if (player == null || !player.Exists()) throw new InvalidOperationException("Player character is unavailable");
                ped.Task.ClearAll();
                ped.Task.FleeFrom(player, 30000);
            });
        }

        public void HealLastPed() => ApplyToLastPed("Pedestrian healed", ped => { ped.Health = ped.MaxHealth; ped.Armor = 100; });
        public void ToggleLastPedInvincibility() => ApplyToLastPed("Pedestrian invincibility toggled", ped => ped.IsInvincible = !ped.IsInvincible);

        private void ApplyToLastPed(string successMessage, Action<Ped> action)
        {
            if (!TryGetLastPed(out var ped)) return;
            try { action(ped); _notifications?.NotifySuccess(successMessage); }
            catch (Exception ex) { Logger.Warn("Pedestrian action failed: " + ex.Message); _notifications?.NotifyError("Pedestrian action failed"); }
        }

        private void ApplyToAllPeds(Action<Ped> action, string successMessage)
        {
            PruneMissingEntities();
            int count = 0;
            foreach (var ped in _spawnedPeds.ToArray())
            {
                if (ped == null || !ped.Exists()) continue;
                try { action(ped); count++; }
                catch (Exception ex) { Logger.Warn("Bulk pedestrian action failed: " + ex.Message); }
            }
            if (count == 0) _notifications?.NotifyInfo("Spawn a pedestrian first");
            else _notifications?.NotifySuccess(successMessage + " (" + count + ")");
        }

        private bool TryGetLastPed(out Ped ped)
        {
            ped = _lastPed;
            if (ped != null && ped.Exists()) return true;
            _lastPed = FindLastExisting(_spawnedPeds);
            ped = _lastPed;
            if (ped != null && ped.Exists()) return true;
            _notifications?.NotifyInfo("Spawn a pedestrian first");
            return false;
        }

        private bool TryGetLastProp(out Prop prop)
        {
            prop = _lastProp;
            if (prop != null && prop.Exists()) return true;
            _lastProp = FindLastExisting(_spawnedProps);
            prop = _lastProp;
            _lastPropModel = null;
            if (prop != null && prop.Exists()) { _propModels.TryGetValue(prop.Handle, out _lastPropModel); return true; }
            _notifications?.NotifyInfo("Spawn an object first");
            return false;
        }

        private void PruneMissingEntities()
        {
            _spawnedProps.RemoveAll(item => item == null || !item.Exists());
            _spawnedPeds.RemoveAll(item => item == null || !item.Exists());
            var liveHandles = new HashSet<int>();
            foreach (var prop in _spawnedProps) if (prop != null && prop.Exists()) liveHandles.Add(prop.Handle);
            var staleHandles = new List<int>();
            foreach (var pair in _propModels) if (!liveHandles.Contains(pair.Key)) staleHandles.Add(pair.Key);
            foreach (int handle in staleHandles) _propModels.Remove(handle);
        }

        private static T FindLastExisting<T>(List<T> entities) where T : Entity
        {
            for (int i = entities.Count - 1; i >= 0; i--)
                if (entities[i] != null && entities[i].Exists()) return entities[i];
            return null;
        }

        private int DeleteTracked<T>(List<T> entities) where T : Entity
        {
            int removed = 0;
            for (int i = entities.Count - 1; i >= 0; i--)
            {
                try
                {
                    T entity = entities[i];
                    if (entity != null && entity.Exists()) { entity.Delete(); removed++; }
                }
                catch (Exception ex) { Logger.Warn("Bulk entity cleanup failed: " + ex.Message); }
            }
            entities.Clear();
            return removed;
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
            _spawnedProps.Clear();
            _spawnedPeds.Clear();
            _propModels.Clear();
        }
    }
}
