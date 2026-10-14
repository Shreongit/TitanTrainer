using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TitanTrainer.Core;

namespace TitanTrainer.Vehicles
{
    /// <summary>
    /// Loads and exposes the vehicle dataset from Data/vehicles.json.
    /// Loads once and caches results for fast searches.
    /// </summary>
    public sealed class VehicleDatabase : IDisposable
    {
        private readonly string _jsonPath;
        private readonly List<VehicleDefinition> _vehicles;

        /// <summary>
        /// Creates and loads the vehicle database from a relative path (e.g., "Data/vehicles.json").
        /// </summary>
        /// <param name="relativeJsonPath">Path relative to AppDomain.CurrentDomain.BaseDirectory.</param>
        public VehicleDatabase(string relativeJsonPath)
        {
            if (string.IsNullOrWhiteSpace(relativeJsonPath))
            {
                throw new ArgumentNullException(nameof(relativeJsonPath));
            }

            _jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativeJsonPath);
            _vehicles = LoadVehicles(_jsonPath);
        }

        /// <summary>
        /// Returns all loaded vehicles as a read-only collection.
        /// </summary>
        public IReadOnlyCollection<VehicleDefinition> AllVehicles => _vehicles.AsReadOnly();

        /// <summary>
        /// Attempts to find a vehicle by exact model name (case-insensitive).
        /// </summary>
        /// <param name="modelName">Model name to find (e.g., "adder").</param>
        /// <returns>The matching VehicleDefinition or null if not found.</returns>
        public VehicleDefinition FindByModel(string modelName)
        {
            if (string.IsNullOrWhiteSpace(modelName)) return null;
            return _vehicles.FirstOrDefault(v => string.Equals(v.ModelName, modelName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Searches vehicles by a query string that matches display name, model name, or category (case-insensitive).
        /// Returns all vehicles when query is null or whitespace.
        /// </summary>
        /// <param name="query">Search query.</param>
        /// <returns>Enumerable of matching vehicles.</returns>
        public IEnumerable<VehicleDefinition> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return _vehicles;
            var q = query.Trim().ToLowerInvariant();
            return _vehicles.Where(v =>
                (!string.IsNullOrEmpty(v.DisplayName) && v.DisplayName.ToLowerInvariant().Contains(q)) ||
                (!string.IsNullOrEmpty(v.ModelName) && v.ModelName.ToLowerInvariant().Contains(q)) ||
                (!string.IsNullOrEmpty(v.Category) && v.Category.ToLowerInvariant().Contains(q))
            );
        }

        /// <summary>
        /// Returns vehicle categories present in the database, ordered alphabetically.
        /// </summary>
        public IEnumerable<string> GetCategories()
        {
            return _vehicles.Select(v => v.Category)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(c => c, StringComparer.OrdinalIgnoreCase);
        }

        private List<VehicleDefinition> LoadVehicles(string fullPath)
        {
            try
            {
                if (!File.Exists(fullPath))
                {
                    Logger.Warn($"Vehicle JSON not found at {fullPath}. Creating a small sample file.");
                    var sample = CreateSampleVehicles();
                    SafeWriteJson(fullPath, sample);
                    return sample;
                }

                var json = File.ReadAllText(fullPath);
                var list = JsonConvert.DeserializeObject<List<VehicleDefinition>>(json);
                if (list == null)
                {
                    Logger.Warn($"Deserialized vehicles.json returned null. Returning empty list.");
                    return new List<VehicleDefinition>();
                }

                var validated = ValidateAndNormalize(list);
                Logger.Info($"Loaded {validated.Count} vehicle definitions from {fullPath}");
                return validated;
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to load vehicle database: " + ex);
                return new List<VehicleDefinition>();
            }
        }

        private static List<VehicleDefinition> ValidateAndNormalize(IEnumerable<VehicleDefinition> source)
        {
            var outList = new List<VehicleDefinition>();
            foreach (var v in source)
            {
                if (v == null) continue;
                // Basic validation: require a model name and display name
                if (string.IsNullOrWhiteSpace(v.ModelName) || string.IsNullOrWhiteSpace(v.DisplayName))
                {
                    Logger.Warn($"Skipping invalid vehicle entry (missing model or display name): {JsonConvert.SerializeObject(v)}");
                    continue;
                }

                // Normalize category
                if (string.IsNullOrWhiteSpace(v.Category))
                {
                    v.Category = "Uncategorized";
                }

                outList.Add(v);
            }

            return outList;
        }

        private static void SafeWriteJson(string path, IEnumerable<VehicleDefinition> data)
        {
            try
            {
                var dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                Logger.Warn("Failed to write sample vehicles.json: " + ex);
            }
        }

        private static List<VehicleDefinition> CreateSampleVehicles()
        {
            return new List<VehicleDefinition>
            {
                new VehicleDefinition { DisplayName = "Adder", ModelName = "adder", Category = "Super", IsDlc = false },
                new VehicleDefinition { DisplayName = "Comet", ModelName = "comet2", Category = "Sports", IsDlc = false },
                new VehicleDefinition { DisplayName = "Bati 801", ModelName = "bati", Category = "Motorcycle", IsDlc = false }
            };
        }

        public void Dispose()
        {
            // Nothing to dispose now; left for future extension (file watchers, db connections etc.)
        }
    }
}