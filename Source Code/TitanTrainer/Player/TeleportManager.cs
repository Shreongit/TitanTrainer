using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GTA;
using GTA.Math;
using GTA.Native;
using Newtonsoft.Json;
using TitanTrainer.Core;
using TitanTrainer.Utils;

namespace TitanTrainer.Player
{
    /// <summary>Persistent named location snapshots for Story Mode teleportation.</summary>
    public sealed class SavedLocation
    {
        public string Name { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float Heading { get; set; }
        public DateTime SavedUtc { get; set; }

        [JsonIgnore]
        public Vector3 Position => new Vector3(X, Y, Z);
    }

    public sealed class TeleportManager
    {
        private readonly NotificationService _notifications;
        private readonly string _locationsPath;
        private readonly List<SavedLocation> _locations;
        private Vector3? _lastPosition;
        private float _lastHeading;

        public TeleportManager(NotificationService notifications, string locationsPath = "Data/saved-locations.json")
        {
            _notifications = notifications;
            _locationsPath = locationsPath;
            _locations = LoadLocations();
        }

        public IReadOnlyList<SavedLocation> SavedLocations => _locations.AsReadOnly();

        public void TeleportTo(Vector3 position)
        {
            TeleportTo(position, null);
        }

        public void TeleportTo(Vector3 position, float? heading)
        {
            try
            {
                Ped ped = Game.Player.Character;
                if (ped == null || !ped.Exists())
                {
                    _notifications?.NotifyError("Player character is unavailable.");
                    return;
                }

                RememberCurrentPosition();
                Entity target = ped.IsInVehicle() ? (Entity)ped.CurrentVehicle : ped;
                if (target == null || !target.Exists()) target = ped;
                target.Position = position;
                if (heading.HasValue) target.Heading = heading.Value;

                _notifications?.NotifySuccess("Teleported");
                Logger.Info($"Teleported to {position} (heading {heading?.ToString("0.0") ?? "unchanged"})");
            }
            catch (Exception ex)
            {
                Logger.Error($"Teleport failed: {ex}");
                _notifications?.NotifyError("Teleport failed.");
            }
        }

        public bool SaveCurrentLocation(string requestedName = null)
        {
            try
            {
                Ped ped = Game.Player.Character;
                if (ped == null || !ped.Exists())
                {
                    _notifications?.NotifyError("Player character is unavailable.");
                    return false;
                }

                Entity target = ped.IsInVehicle() ? (Entity)ped.CurrentVehicle : ped;
                if (target == null || !target.Exists()) target = ped;

                string name = string.IsNullOrWhiteSpace(requestedName) ? GetNextLocationName() : requestedName.Trim();
                if (name.Length > 48) name = name.Substring(0, 48).Trim();
                if (string.IsNullOrWhiteSpace(name)) name = GetNextLocationName();
                if (_locations.Any(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)))
                {
                    _notifications?.NotifyError("A saved location with that name already exists.");
                    return false;
                }

                Vector3 pos = target.Position;
                _locations.Add(new SavedLocation
                {
                    Name = name,
                    X = pos.X,
                    Y = pos.Y,
                    Z = pos.Z,
                    Heading = target.Heading,
                    SavedUtc = DateTime.UtcNow
                });
                PersistLocations();
                _notifications?.NotifySuccess("Saved location: " + name);
                Logger.Info($"Saved location '{name}' at {pos}, heading {target.Heading:0.0}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Saving location failed: " + ex);
                _notifications?.NotifyError("Could not save location.");
                return false;
            }
        }

        public bool DeleteLocation(string name)
        {
            int removed = _locations.RemoveAll(x => x != null && string.Equals(x.Name, name, StringComparison.Ordinal));
            if (removed == 0) return false;
            PersistLocations();
            _notifications?.NotifyInfo("Deleted location: " + name);
            return true;
        }

        public bool TeleportToSavedLocation(string name)
        {
            SavedLocation location = _locations.FirstOrDefault(x => x != null && string.Equals(x.Name, name, StringComparison.Ordinal));
            if (location == null)
            {
                _notifications?.NotifyError("Saved location not found.");
                return false;
            }
            TeleportTo(location.Position, location.Heading);
            _notifications?.NotifyInfo("Location: " + location.Name);
            return true;
        }

        public void TeleportToLastPosition()
        {
            if (!_lastPosition.HasValue)
            {
                _notifications?.NotifyInfo("No previous teleport position is available yet.");
                return;
            }
            Vector3 destination = _lastPosition.Value;
            float heading = _lastHeading;
            _lastPosition = null;
            TeleportTo(destination, heading);
            _notifications?.NotifyInfo("Returned to previous position");
        }

        public void TeleportToWaypoint()
        {
            try
            {
                int blip = Function.Call<int>(Hash.GET_FIRST_BLIP_INFO_ID, 8);
                if (!Function.Call<bool>(Hash.DOES_BLIP_EXIST, blip))
                {
                    _notifications?.NotifyError("No waypoint found.");
                    return;
                }
                Vector3 coords = Function.Call<Vector3>(Hash.GET_BLIP_COORDS, blip);
                // Start above the waypoint to give the world time to stream collision.
                TeleportTo(new Vector3(coords.X, coords.Y, coords.Z + 5f));
            }
            catch (Exception ex)
            {
                Logger.Error("Waypoint teleport failed: " + ex);
                _notifications?.NotifyError("Waypoint teleport failed.");
            }
        }

        public void TeleportForward(float distance = 10f)
        {
            Ped ped = Game.Player.Character;
            if (ped == null || !ped.Exists()) return;
            TeleportTo(ped.Position + ped.ForwardVector * distance);
        }

        public void TeleportSky()
        {
            Ped ped = Game.Player.Character;
            if (ped == null || !ped.Exists()) return;
            Vector3 pos = ped.Position;
            TeleportTo(new Vector3(pos.X, pos.Y, pos.Z + 200f));
        }

        public void TeleportSafe()
        {
            Ped ped = Game.Player.Character;
            if (ped == null || !ped.Exists()) return;
            Vector3 pos = ped.Position;
            TeleportTo(new Vector3(pos.X, pos.Y, pos.Z + 2f));
        }

        private void RememberCurrentPosition()
        {
            try
            {
                Ped ped = Game.Player.Character;
                if (ped == null || !ped.Exists()) return;
                Entity target = ped.IsInVehicle() ? (Entity)ped.CurrentVehicle : ped;
                if (target == null || !target.Exists()) target = ped;
                _lastPosition = target.Position;
                _lastHeading = target.Heading;
            }
            catch (Exception ex) { Logger.Warn("Could not remember teleport origin: " + ex.Message); }
        }

        private string GetNextLocationName()
        {
            int number = 1;
            string name;
            do { name = "Saved Location " + number.ToString("00"); number++; }
            while (_locations.Any(x => x != null && string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)));
            return name;
        }

        private List<SavedLocation> LoadLocations()
        {
            try
            {
                string directory = Path.GetDirectoryName(_locationsPath);
                if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
                if (!File.Exists(_locationsPath)) { File.WriteAllText(_locationsPath, "[]"); return new List<SavedLocation>(); }
                var result = JsonConvert.DeserializeObject<List<SavedLocation>>(File.ReadAllText(_locationsPath)) ?? new List<SavedLocation>();
                return result.Where(x => x != null && !string.IsNullOrWhiteSpace(x.Name) && IsFinite(x.X) && IsFinite(x.Y) && IsFinite(x.Z) && IsFinite(x.Heading)).Take(250).ToList();
            }
            catch (Exception ex)
            {
                Logger.Warn("Saved locations could not be loaded: " + ex.Message);
                return new List<SavedLocation>();
            }
        }

        private void PersistLocations()
        {
            string directory = Path.GetDirectoryName(_locationsPath);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            string temporary = _locationsPath + ".tmp";
            File.WriteAllText(temporary, JsonConvert.SerializeObject(_locations, Formatting.Indented));
            if (File.Exists(_locationsPath)) File.Delete(_locationsPath);
            File.Move(temporary, _locationsPath);
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
