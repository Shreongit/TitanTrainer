using System;
using GTA;
using GTA.Native;
using TitanTrainer.Core;

namespace TitanTrainer.Vehicles
{
    /// <summary>
    /// Responsible for requesting models, spawning vehicles, and applying options.
    /// </summary>
    public sealed class VehicleSpawner
    {
        private const int ModelRequestTimeoutMs = 2000;

        public VehicleSpawner()
        {
        }

        public Vehicle Spawn(VehicleDefinition def, bool spawnInside = true, bool applyMaxUpgrade = false)
        {
            if (def == null) throw new ArgumentNullException(nameof(def));

            try
            {
                var modelName = def.ModelName;
                if (string.IsNullOrWhiteSpace(modelName))
                {
                    Logger.Warn($"VehicleDefinition for {def.DisplayName} has no model name");
                    return null;
                }

                var model = new Model(modelName);
                if (!model.IsValid)
                {
                    Logger.Warn($"Model {modelName} is not valid.");
                    return null;
                }

                model.Request();
                var waited = 0;
                while (!model.IsLoaded && waited < ModelRequestTimeoutMs)
                {
                    Script.Wait(10);
                    waited += 10;
                }

                if (!model.IsLoaded)
                {
                    Logger.Error($"Failed to load model '{modelName}' after {ModelRequestTimeoutMs}ms.");
                    return null;
                }

                var player = Game.Player.Character;
                var offset = player.ForwardVector * 5.0f;
                var spawnPos = player.Position + offset;
                var heading = player.Heading;

                // Use Vehicle.Create per SHVDN3 guidance to avoid obsolete World.CreateVehicle
                var vehicle = Vehicle.Create(model, spawnPos, heading);
                if (vehicle == null || !vehicle.Exists())
                {
                    Logger.Error($"Vehicle.Create returned null for model '{modelName}'.");
                    model.MarkAsNoLongerNeeded();
                    return null;
                }

                vehicle.Repair();
                vehicle.IsPersistent = true;

                if (applyMaxUpgrade)
                {
                    try
                    {
                        Function.Call(Hash.SET_VEHICLE_MOD_KIT, vehicle, 0);
                        for (int modType = 0; modType <= 49; modType++)
                        {
                            var modCount = Function.Call<int>(Hash.GET_NUM_VEHICLE_MODS, vehicle, modType);
                            if (modCount > 0)
                            {
                                Function.Call(Hash.SET_VEHICLE_MOD, vehicle, modType, modCount - 1, false);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn("Failed to apply max upgrades: " + ex);
                    }
                }

                if (spawnInside)
                {
                    Game.Player.Character.SetIntoVehicle(vehicle, VehicleSeat.Driver);
                }

                Logger.Info($"Spawned vehicle {def.DisplayName} ({def.ModelName}) at {spawnPos}.");
                model.MarkAsNoLongerNeeded();
                return vehicle;
            }
            catch (Exception ex)
            {
                Logger.Error("Vehicle spawn failed: " + ex);
                return null;
            }
        }
    }
}