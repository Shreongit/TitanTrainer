using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GTA;
using GTA.Math;
using Newtonsoft.Json;
using TitanTrainer.Core;
using TitanTrainer.Utils;

namespace TitanTrainer.World
{
    public sealed class SceneEntityData
    {
        public string Type { get; set; }
        public int ModelHash { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float Pitch { get; set; }
        public float Roll { get; set; }
        public float Heading { get; set; }
    }

    public sealed class SceneDefinition
    {
        public string Name { get; set; }
        public DateTime CreatedUtc { get; set; }
        public List<SceneEntityData> Entities { get; set; } = new List<SceneEntityData>();
    }

    /// <summary>Persists and restores scene snapshots made from Titan Trainer-spawned entities and the player's current vehicle.</summary>
    public sealed class SceneBuilderService
    {
        private const int MaxEntitiesPerScene = 100;
        private readonly EntitySpawner _entitySpawner;
        private readonly NotificationService _notifications;
        private readonly string _path;
        private readonly List<Entity> _loadedEntities = new List<Entity>();
        private List<SceneDefinition> _scenes;

        public SceneBuilderService(EntitySpawner entitySpawner, NotificationService notifications, string path = "Data/scenes.json")
        {
            _entitySpawner = entitySpawner ?? throw new ArgumentNullException(nameof(entitySpawner));
            _notifications = notifications;
            _path = path;
            _scenes = LoadFromDisk();
        }

        public IReadOnlyList<SceneDefinition> Scenes => _scenes.AsReadOnly();

        public string SaveCurrentScene()
        {
            try
            {
                var scene = new SceneDefinition
                {
                    Name = "Scene " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"),
                    CreatedUtc = DateTime.UtcNow,
                    Entities = _entitySpawner.CaptureTrackedEntities()
                };

                var player = Game.Player.Character;
                if (player != null && player.Exists() && player.IsInVehicle())
                {
                    Vehicle vehicle = player.CurrentVehicle;
                    if (vehicle != null && vehicle.Exists()) scene.Entities.Add(Capture(vehicle, "vehicle"));
                }

                if (scene.Entities.Count == 0)
                {
                    _notifications?.NotifyError("Nothing to save. Spawn props/peds or enter a vehicle first.");
                    return null;
                }

                if (scene.Entities.Count > MaxEntitiesPerScene)
                    scene.Entities = scene.Entities.Take(MaxEntitiesPerScene).ToList();

                _scenes.Add(scene);
                Persist();
                _notifications?.NotifySuccess("Saved scene: " + scene.Name + " (" + scene.Entities.Count + " entities)");
                return scene.Name;
            }
            catch (Exception ex)
            {
                Logger.Error("Scene save failed: " + ex);
                _notifications?.NotifyError("Could not save scene");
                return null;
            }
        }

        public bool LoadScene(string name)
        {
            var scene = _scenes.FirstOrDefault(s => s != null && string.Equals(s.Name, name, StringComparison.Ordinal));
            if (scene == null || scene.Entities == null) { _notifications?.NotifyError("Scene not found"); return false; }
            int loaded = 0;
            try
            {
                foreach (var data in scene.Entities.Take(MaxEntitiesPerScene))
                {
                    try
                    {
                        var model = new Model(data.ModelHash);
                        if (!model.IsValid || !model.IsInCdImage) continue;
                        if (data.Type == "ped" && !model.IsPed) continue;
                        if (data.Type == "vehicle" && !model.IsVehicle) continue;
                        model.Request(1500);
                        if (!model.IsLoaded) { model.MarkAsNoLongerNeeded(); continue; }

                        Vector3 position = new Vector3(data.X, data.Y, data.Z);
                        Entity entity = null;
                        if (data.Type == "prop") entity = Prop.Create(model, position, true, true);
                        else if (data.Type == "ped") entity = Ped.Create(model, position, data.Heading);
                        else if (data.Type == "vehicle") entity = Vehicle.Create(model, position, data.Heading);
                        model.MarkAsNoLongerNeeded();
                        if (entity == null || !entity.Exists()) continue;
                        entity.Rotation = new Vector3(data.Pitch, data.Roll, data.Heading);
                        entity.Heading = data.Heading;
                        if (entity is Ped ped) { ped.IsPersistent = true; ped.BlockPermanentEvents = true; }
                        _loadedEntities.Add(entity);
                        loaded++;
                    }
                    catch (Exception ex) { Logger.Warn("Scene entity skipped: " + ex.Message); }
                }
                PruneLoadedEntities();
                _notifications?.NotifyInfo("Loaded scene: " + scene.Name + " (" + loaded + "/" + scene.Entities.Count + ")");
                return loaded > 0;
            }
            catch (Exception ex)
            {
                Logger.Error("Scene load failed: " + ex);
                _notifications?.NotifyError("Could not load scene");
                return false;
            }
        }

        public bool DeleteScene(string name)
        {
            int removed = _scenes.RemoveAll(s => string.Equals(s.Name, name, StringComparison.Ordinal));
            if (removed == 0) return false;
            Persist();
            _notifications?.NotifySuccess("Deleted scene: " + name);
            return true;
        }

        public int DeleteLoadedEntities()
        {
            int removed = 0;
            for (int i = _loadedEntities.Count - 1; i >= 0; i--)
            {
                try { var entity = _loadedEntities[i]; if (entity != null && entity.Exists()) { entity.Delete(); removed++; } }
                catch (Exception ex) { Logger.Warn("Loaded scene cleanup failed: " + ex.Message); }
            }
            _loadedEntities.Clear();
            _notifications?.NotifyInfo("Removed " + removed + " entity/entities loaded by Scene Builder");
            return removed;
        }

        private static SceneEntityData Capture(Entity entity, string type)
        {
            var p = entity.Position;
            var r = entity.Rotation;
            return new SceneEntityData { Type = type, ModelHash = entity.Model.Hash, X = p.X, Y = p.Y, Z = p.Z, Pitch = r.X, Roll = r.Y, Heading = entity.Heading };
        }

        private List<SceneDefinition> LoadFromDisk()
        {
            try
            {
                if (!File.Exists(_path)) { Directory.CreateDirectory(Path.GetDirectoryName(_path) ?? "."); File.WriteAllText(_path, "[]"); }
                var loaded = JsonConvert.DeserializeObject<List<SceneDefinition>>(File.ReadAllText(_path)) ?? new List<SceneDefinition>();
                return loaded.Where(s => s != null && !string.IsNullOrWhiteSpace(s.Name) && s.Entities != null).ToList();
            }
            catch (Exception ex) { Logger.Warn("Scene catalogue could not be read: " + ex.Message); return new List<SceneDefinition>(); }
        }

        private void Persist()
        {
            try
            {
                string directory = Path.GetDirectoryName(_path);
                if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
                string temporary = _path + ".tmp";
                File.WriteAllText(temporary, JsonConvert.SerializeObject(_scenes, Formatting.Indented));
                if (File.Exists(_path)) File.Delete(_path);
                File.Move(temporary, _path);
            }
            catch (Exception ex) { Logger.Error("Scene catalogue save failed: " + ex); throw; }
        }

        private void PruneLoadedEntities() => _loadedEntities.RemoveAll(e => e == null || !e.Exists());
    }
}
