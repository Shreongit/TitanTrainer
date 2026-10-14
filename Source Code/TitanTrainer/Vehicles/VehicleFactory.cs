using System;
using GTA;
using GTA.Native;
using TitanTrainer.Core;

namespace TitanTrainer.Vehicles
{
    /// <summary>
    /// Helper methods to apply spawn options to a spawned vehicle.
    /// Kept minimal to avoid depending on Vehicle properties not present in SHVDN3.
    /// </summary>
    public static class VehicleFactory
    {
        public static void ApplyOptions(Vehicle vehicle, SpawnOptions options)
        {
            if (vehicle == null || options == null) return;

            try
            {
                if (options.Repair) vehicle.Repair();
                if (options.Clean) vehicle.DirtLevel = 0f;

                vehicle.IsEngineRunning = options.EngineRunning;

                if (options.BulletproofTires)
                {
                    vehicle.CanTiresBurst = false;
                }

                if (!string.IsNullOrEmpty(options.PlateText))
                {
                    try
                    {
                        vehicle.Mods.LicensePlate = options.PlateText;
                    }
                    catch
                    {
                        // Fallback to native
                        Function.Call(Hash.SET_VEHICLE_NUMBER_PLATE_TEXT, vehicle, options.PlateText);
                    }
                }

                if (options.MaxUpgrade)
                {
                    ApplyMaxUpgradesBestEffort(vehicle);
                }

                // Xenon and neon are left as best-effort - attempt via natives if requested
                if (options.XenonLights)
                {
                    try
                    {
                        // Example: toggling mod type for xenon might require specific native calls per vehicle.
                        Function.Call(Hash.TOGGLE_VEHICLE_MOD, vehicle, 22, true);
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                Logger.Warn("Failed to apply vehicle options: " + ex);
            }
        }

        private static void ApplyMaxUpgradesBestEffort(Vehicle vehicle)
        {
            try
            {
                Function.Call(Hash.SET_VEHICLE_MOD_KIT, vehicle, 0);
                for (int modType = 0; modType <= 49; modType++)
                {
                    var count = Function.Call<int>(Hash.GET_NUM_VEHICLE_MODS, vehicle, modType);
                    if (count > 0)
                    {
                        Function.Call(Hash.SET_VEHICLE_MOD, vehicle, modType, count - 1, false);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Warn("ApplyMaxUpgrades failed: " + ex);
            }
        }
    }

    public sealed class SpawnOptions
    {
        public bool SpawnInside { get; set; } = true;
        public bool MaxUpgrade { get; set; } = false;
        public bool Repair { get; set; } = true;
        public bool Clean { get; set; } = true;
        public bool EngineRunning { get; set; } = true;
        public bool BulletproofTires { get; set; } = false;
        public bool XenonLights { get; set; } = false;
        public string PlateText { get; set; } = string.Empty;
    }
}