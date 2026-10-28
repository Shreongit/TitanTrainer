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

            Model model = default(Model);
            bool modelCreated = false;
            try
            {
                var modelName = def.ModelName;
                if (string.IsNullOrWhiteSpace(modelName))
                {
                    Logger.Warn($"VehicleDefinition for {def.DisplayName} has no model name");
                    return null;
                }

                model = new Model(modelName);
                modelCreated = true;
                if (!model.IsValid || !model.IsVehicle)
                {
                    Logger.Warn($"Model '{modelName}' is not a valid vehicle model.");
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
                if (player == null || !player.Exists())
                {
                    Logger.Warn("Cannot spawn a vehicle because the player character is unavailable.");
                    return null;
                }

                var spawnPos = player.Position + player.ForwardVector * 5.0f;
                var vehicle = Vehicle.Create(model, spawnPos, player.Heading);
                if (vehicle == null || !vehicle.Exists())
                {
                    Logger.Error($"Vehicle.Create returned no valid vehicle for model '{modelName}'.");
                    return null;
                }

                vehicle.Repair();
                vehicle.IsPersistent = true;

                if (applyMaxUpgrade)
                {
                    try
                    {
                        Function.Call(Hash.SET_VEHICLE_MOD_KIT, vehicle.Handle, 0);
                        for (int modType = 0; modType < 50; modType++)
                        {
                            var modCount = Function.Call<int>(Hash.GET_NUM_VEHICLE_MODS, vehicle.Handle, modType);
                            if (modCount > 0)
                            {
                                Function.Call(Hash.SET_VEHICLE_MOD, vehicle.Handle, modType, modCount - 1, false);
                            }
                        }
                        Function.Call(Hash.TOGGLE_VEHICLE_MOD, vehicle.Handle, 18, true);
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn("Vehicle spawned, but maximum upgrades could not all be applied: " + ex.Message);
                    }
                }

                if (spawnInside)
                {
                    player.SetIntoVehicle(vehicle, VehicleSeat.Driver);
                }

                Logger.Info($"Spawned vehicle {def.DisplayName} ({modelName}) at {spawnPos}.");
                return vehicle;
            }
            catch (Exception ex)
            {
                Logger.Error("Vehicle spawn failed: " + ex);
                return null;
            }
            finally
            {
                // Release the requested model on every exit path, including errors/timeouts.
                try
                {
                    if (modelCreated) model.MarkAsNoLongerNeeded();
                }
                catch (Exception ex)
                {
                    Logger.Warn("Could not release vehicle model: " + ex.Message);
                }
            }
        }

    }
}