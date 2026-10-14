using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TitanTrainer.Core;
using TitanTrainer.Utils;

namespace TitanTrainer.Vehicles
{
    /// <summary>
    /// High-level vehicle manager handling favorites and spawn history.
    /// Delegates actual spawn to VehicleSpawner.
    /// </summary>
    public sealed class VehicleManager : IDisposable
    {
        private readonly VehicleDatabase _database;
        private readonly VehicleSpawner _spawner;
        private readonly string _favoritesPath;
        private readonly List<VehicleDefinition> _favorites;
        private readonly List<VehicleDefinition> _spawnHistory;
        private readonly NotificationService _notificationService;
        private readonly VehicleHealth _health;
        private readonly VehiclePerformance _performance;
        private readonly VehicleControls _controls;
        private readonly VehicleAppearance _appearance;
        private readonly VehicleUtility _utility;
        

        public FavoriteManager FavoriteManager { get; }

        /// <summary>
        /// Primary constructor that takes the vehicle database.
        /// </summary>
        public VehicleManager(VehicleDatabase database)
            : this(database, null)
        {
        }

        /// <summary>
        /// Compatibility constructor: accepts a NotificationService if provided.
        /// Kept to match ScriptMain's construction call.
        /// </summary>
        public VehicleManager(VehicleDatabase database, NotificationService notificationService)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _notificationService = notificationService; // may be null

            _spawner = new VehicleSpawner();
            _health = new VehicleHealth(_notificationService);
            _performance = new VehiclePerformance(_notificationService);
            _controls = new VehicleControls(_notificationService);
            _appearance = new VehicleAppearance(_notificationService);
            _utility = new VehicleUtility(_notificationService);

            _favoritesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "favorites.json");
            _favorites = LoadFavorites();
            FavoriteManager = new FavoriteManager(_favorites, _favoritesPath);

            _spawnHistory = new List<VehicleDefinition>();
        }

        private List<VehicleDefinition> LoadFavorites()
        {
            try
            {
                if (!File.Exists(_favoritesPath)) return new List<VehicleDefinition>();
                var json = File.ReadAllText(_favoritesPath);
                var list = JsonConvert.DeserializeObject<List<VehicleDefinition>>(json);
                return list ?? new List<VehicleDefinition>();
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to load favorites: " + ex);
                return new List<VehicleDefinition>();
            }
        }

        public void SpawnVehicle(VehicleDefinition def, bool spawnInside = true)
        {
            if (def == null) return;
            try
            {
                var veh = _spawner.Spawn(def, spawnInside);
                if (veh != null)
                {
                    _spawnHistory.Insert(0, def);
                    if (_spawnHistory.Count > 30) _spawnHistory.RemoveAt(_spawnHistory.Count - 1);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("SpawnVehicle failed: " + ex);
                _notificationService?.NotifyError("Failed to spawn vehicle. See logs.");
            }
        }

        public VehicleDatabase Database => _database;
        public VehicleHealth Health => _health;

        public VehiclePerformance Performance => _performance;

        public VehicleControls Controls => _controls;

        public VehicleAppearance Appearance => _appearance;

        public VehicleUtility Utility => _utility;

        public IEnumerable<VehicleDefinition> GetSpawnHistory() => _spawnHistory.AsReadOnly();

        
        public void Update()
        {
            _health.Update();

            _performance.Update();

            _controls.Update();

            _appearance.Update();
        }
        
        public void Dispose()
        {
            // nothing to dispose currently
        }
    }

    public sealed class FavoriteManager
    {
        private readonly List<VehicleDefinition> _favorites;
        private readonly string _path;

        public FavoriteManager(List<VehicleDefinition> favorites, string path)
        {
            _favorites = favorites ?? new List<VehicleDefinition>();
            _path = path ?? throw new ArgumentNullException(nameof(path));
        }

        public void AddFavorite(VehicleDefinition def)
        {
            if (def == null) return;
            if (_favorites.Any(f => string.Equals(f.ModelName, def.ModelName, StringComparison.OrdinalIgnoreCase))) return;
            _favorites.Add(def);
            Save();
            Logger.Info($"Added favorite: {def.DisplayName}");
        }

        public void RemoveFavorite(VehicleDefinition def)
        {
            if (def == null) return;
            _favorites.RemoveAll(f => string.Equals(f.ModelName, def.ModelName, StringComparison.OrdinalIgnoreCase));
            Save();
            Logger.Info($"Removed favorite: {def.DisplayName}");
        }

        public IReadOnlyList<VehicleDefinition> GetFavorites() => _favorites.AsReadOnly();

        private void Save()
        {
            try
            {
                var dir = Path.GetDirectoryName(_path);
                if (!string.IsNullOrEmpty(dir)) System.IO.Directory.CreateDirectory(dir);
                var json = JsonConvert.SerializeObject(_favorites, Formatting.Indented);
                File.WriteAllText(_path, json);
                Logger.Info($"Saved favorites to {_path}");
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to save favorites: " + ex);
            }
        }
    }
}