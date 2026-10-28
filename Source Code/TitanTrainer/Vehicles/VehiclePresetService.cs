using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GTA;
using GTA.Native;
using Newtonsoft.Json;
using TitanTrainer.Core;
using TitanTrainer.Utils;

namespace TitanTrainer.Vehicles
{
    /// <summary>Persists vehicle presets and favorites as JSON files under the trainer Data folder.</summary>
    public sealed class VehiclePresetService
    {
        private readonly string _presetsPath;
        private readonly string _favoritesPath;
        private readonly NotificationService _notifications;
        private readonly VehicleDatabase _database;
        private readonly List<VehiclePreset> _presets;
        private readonly List<VehicleFavorite> _favorites;

        public IReadOnlyList<VehiclePreset> Presets => _presets.AsReadOnly();
        public IReadOnlyList<VehicleFavorite> Favorites => _favorites.AsReadOnly();

        public VehiclePresetService(string dataDirectory, VehicleDatabase database, NotificationService notifications)
        {
            if (string.IsNullOrWhiteSpace(dataDirectory)) throw new ArgumentNullException(nameof(dataDirectory));
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _notifications = notifications;
            Directory.CreateDirectory(dataDirectory);
            _presetsPath = Path.Combine(dataDirectory, "vehicle-presets.json");
            _favoritesPath = Path.Combine(dataDirectory, "favorites.json");
            _presets = ReadList<VehiclePreset>(_presetsPath);
            _favorites = ReadList<VehicleFavorite>(_favoritesPath);
        }

        public bool SaveCurrentVehicle()
        {
            var vehicle = GetCurrentVehicle();
            if (vehicle == null)
            {
                _notifications?.NotifyError("Enter a vehicle before saving a preset.");
                return false;
            }

            try
            {
                uint hash = unchecked((uint)vehicle.Model.Hash);
                var definition = _database.AllVehicles.FirstOrDefault(v => v.Hash.HasValue && v.Hash.Value == hash);
                // The catalogue's model name is useful for display; the hash is authoritative for spawning.
                var name = definition?.DisplayName;
                if (string.IsNullOrWhiteSpace(name)) name = "Vehicle " + hash.ToString("X8");
                var preset = new VehiclePreset
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Name = name + " - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                    ModelHash = hash,
                    PrimaryColor = (int)vehicle.Mods.PrimaryColor,
                    SecondaryColor = (int)vehicle.Mods.SecondaryColor,
                    EngineHealth = Function.Call<float>(Hash.GET_VEHICLE_ENGINE_HEALTH, vehicle.Handle),
                    BodyHealth = Function.Call<float>(Hash.GET_VEHICLE_BODY_HEALTH, vehicle.Handle),
                    DirtLevel = vehicle.DirtLevel,
                    Mods = new int[50],
                    ToggleMods = new bool[50]
                };

                Function.Call(Hash.SET_VEHICLE_MOD_KIT, vehicle.Handle, 0);
                for (int slot = 0; slot < preset.Mods.Length; slot++)
                {
                    preset.Mods[slot] = Function.Call<int>(Hash.GET_VEHICLE_MOD, vehicle.Handle, slot);
                    preset.ToggleMods[slot] = Function.Call<bool>(Hash.IS_TOGGLE_MOD_ON, vehicle.Handle, slot);
                }
                _presets.Add(preset);
                SaveList(_presetsPath, _presets);
                _notifications?.NotifySuccess("Vehicle preset saved: " + preset.Name);
                Logger.Info("Saved vehicle preset " + preset.Id + " for model hash " + hash.ToString("X8"));
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Could not save vehicle preset: " + ex);
                _notifications?.NotifyError("Could not save vehicle preset. See trainer.log.");
                return false;
            }
        }

        public bool SpawnPreset(string id)
        {
            var preset = _presets.FirstOrDefault(p => string.Equals(p.Id, id, StringComparison.Ordinal));
            if (preset == null) return false;
            return Spawn(preset.ModelHash, preset);
        }

        public bool SpawnFavorite(string id)
        {
            var favorite = _favorites.FirstOrDefault(f => string.Equals(f.Id, id, StringComparison.Ordinal));
            if (favorite == null) return false;
            return Spawn(favorite.ModelHash, null);
        }

        public bool ToggleFavoriteCurrent()
        {
            var vehicle = GetCurrentVehicle();
            if (vehicle == null)
            {
                _notifications?.NotifyError("Enter a vehicle before changing favorites.");
                return false;
            }

            uint hash = unchecked((uint)vehicle.Model.Hash);
            var existing = _favorites.FirstOrDefault(f => f.ModelHash == hash);
            if (existing != null)
            {
                _favorites.Remove(existing);
                SaveList(_favoritesPath, _favorites);
                _notifications?.NotifyInfo("Removed vehicle from favorites.");
                return true;
            }

            var definition = _database.AllVehicles.FirstOrDefault(v => v.Hash.HasValue && v.Hash.Value == hash);
            var displayName = definition?.DisplayName;
            if (string.IsNullOrWhiteSpace(displayName)) displayName = "Vehicle " + hash.ToString("X8");
            _favorites.Add(new VehicleFavorite { Id = Guid.NewGuid().ToString("N"), Name = displayName, ModelHash = hash });
            SaveList(_favoritesPath, _favorites);
            _notifications?.NotifySuccess("Added " + displayName + " to favorites.");
            return true;
        }

        public bool DeletePreset(string id)
        {
            var preset = _presets.FirstOrDefault(p => string.Equals(p.Id, id, StringComparison.Ordinal));
            if (preset == null) return false;
            _presets.Remove(preset);
            SaveList(_presetsPath, _presets);
            _notifications?.NotifyInfo("Deleted preset: " + preset.Name);
            return true;
        }

        public bool RemoveFavorite(string id)
        {
            var favorite = _favorites.FirstOrDefault(f => string.Equals(f.Id, id, StringComparison.Ordinal));
            if (favorite == null) return false;
            _favorites.Remove(favorite);
            SaveList(_favoritesPath, _favorites);
            _notifications?.NotifyInfo("Removed favorite: " + favorite.Name);
            return true;
        }

        private bool Spawn(uint hash, VehiclePreset preset)
        {
            Model model = default(Model);
            try
            {
                model = new Model(unchecked((int)hash));
                if (!model.IsValid || !model.IsVehicle)
                {
                    _notifications?.NotifyError("Saved vehicle model is no longer valid.");
                    return false;
                }
                model.Request();
                int waited = 0;
                while (!model.IsLoaded && waited < 3000)
                {
                    Script.Wait(10);
                    waited += 10;
                }
                if (!model.IsLoaded)
                {
                    _notifications?.NotifyError("Vehicle model failed to load.");
                    return false;
                }

                var player = Game.Player.Character;
                if (player == null || !player.Exists()) return false;
                var position = player.Position + player.ForwardVector * 5.0f;
                var vehicle = Vehicle.Create(model, position, player.Heading);
                if (vehicle == null || !vehicle.Exists()) return false;
                vehicle.IsPersistent = true;
                vehicle.Repair();

                if (preset != null) ApplyPreset(vehicle, preset);
                player.SetIntoVehicle(vehicle, VehicleSeat.Driver);
                _notifications?.NotifySuccess(preset == null ? "Favorite vehicle spawned." : "Vehicle preset loaded.");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Could not spawn saved vehicle: " + ex);
                _notifications?.NotifyError("Could not spawn saved vehicle.");
                return false;
            }
            finally
            {
                try { model.MarkAsNoLongerNeeded(); } catch { }
            }
        }

        private static void ApplyPreset(Vehicle vehicle, VehiclePreset preset)
        {
            try
            {
                Function.Call(Hash.SET_VEHICLE_MOD_KIT, vehicle.Handle, 0);
                vehicle.Mods.PrimaryColor = (VehicleColor)preset.PrimaryColor;
                vehicle.Mods.SecondaryColor = (VehicleColor)preset.SecondaryColor;
                vehicle.DirtLevel = Math.Max(0f, Math.Min(15f, preset.DirtLevel));
                for (int slot = 0; slot < Math.Min(50, preset.Mods?.Length ?? 0); slot++)
                {
                    int mod = preset.Mods[slot];
                    if (mod >= 0) Function.Call(Hash.SET_VEHICLE_MOD, vehicle.Handle, slot, mod, false);
                }
                for (int slot = 0; slot < Math.Min(50, preset.ToggleMods?.Length ?? 0); slot++)
                    Function.Call(Hash.TOGGLE_VEHICLE_MOD, vehicle.Handle, slot, preset.ToggleMods[slot]);
                Function.Call(Hash.SET_VEHICLE_ENGINE_HEALTH, vehicle.Handle, Math.Max(-4000f, Math.Min(1000f, preset.EngineHealth)));
                Function.Call(Hash.SET_VEHICLE_BODY_HEALTH, vehicle.Handle, Math.Max(0f, Math.Min(1000f, preset.BodyHealth)));
            }
            catch (Exception ex)
            {
                Logger.Warn("Some saved vehicle modifications could not be restored: " + ex.Message);
            }
        }

        private static Vehicle GetCurrentVehicle()
        {
            var character = Game.Player.Character;
            if (character == null || !character.Exists() || !character.IsInVehicle()) return null;
            var vehicle = character.CurrentVehicle;
            return vehicle != null && vehicle.Exists() ? vehicle : null;
        }

        private static List<T> ReadList<T>(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    SaveList(path, new List<T>());
                    return new List<T>();
                }
                return JsonConvert.DeserializeObject<List<T>>(File.ReadAllText(path)) ?? new List<T>();
            }
            catch (Exception ex)
            {
                Logger.Warn("Could not read " + Path.GetFileName(path) + ": " + ex.Message);
                return new List<T>();
            }
        }

        private static void SaveList<T>(string path, List<T> items)
        {
            var temporaryPath = path + ".tmp";
            try
            {
                File.WriteAllText(temporaryPath, JsonConvert.SerializeObject(items, Formatting.Indented));
                if (File.Exists(path)) File.Replace(temporaryPath, path, null);
                else File.Move(temporaryPath, path);
            }
            catch
            {
                try { if (File.Exists(temporaryPath)) File.Delete(temporaryPath); } catch { }
                throw;
            }
        }
    }

    public sealed class VehiclePreset
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public uint ModelHash { get; set; }
        public int PrimaryColor { get; set; }
        public int SecondaryColor { get; set; }
        public float EngineHealth { get; set; }
        public float BodyHealth { get; set; }
        public float DirtLevel { get; set; }
        public int[] Mods { get; set; }
        public bool[] ToggleMods { get; set; }
    }

    public sealed class VehicleFavorite
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public uint ModelHash { get; set; }
    }
}
