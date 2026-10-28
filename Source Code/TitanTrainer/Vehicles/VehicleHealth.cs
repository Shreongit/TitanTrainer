using GTA;
using GTA.Native;
using TitanTrainer.Core;
using TitanTrainer.Utils;

namespace TitanTrainer.Vehicles
{
    public sealed class VehicleHealth
    {
        private readonly NotificationService _notifications;

        private bool _godMode;
        private bool _autoRepair;
        private bool _invincibleTires;
        private Vehicle _lastVehicle;

        public VehicleHealth(NotificationService notifications)
        {
            _notifications = notifications;
        }

        public bool GodMode
        {
            get => _godMode;
            set
            {
                _godMode = value;

                _notifications?.NotifyInfo(
                    value
                        ? "Vehicle God Mode Enabled"
                        : "Vehicle God Mode Disabled");
            }
        }

        public bool AutoRepair
        {
            get => _autoRepair;
            set
            {
                _autoRepair = value;

                _notifications?.NotifyInfo(
                    value
                        ? "Auto Repair Enabled"
                        : "Auto Repair Disabled");
            }
        }

        public bool InvincibleTires
        {
            get => _invincibleTires;
            set
            {
                _invincibleTires = value;

                _notifications?.NotifyInfo(
                    value
                        ? "Invincible Tires Enabled"
                        : "Invincible Tires Disabled");
            }
        }

        public void Repair()
        {
            Vehicle vehicle = Game.Player.Character.CurrentVehicle;

            if (vehicle == null)
                return;

            vehicle.Repair();

            vehicle.EngineHealth = 1000f;
            vehicle.BodyHealth = 1000f;

            _notifications?.NotifySuccess("Vehicle Repaired");

            Logger.Info("Vehicle repaired.");
        }

        public void Clean()
        {
            Vehicle vehicle = Game.Player.Character.CurrentVehicle;

            if (vehicle == null)
                return;

            vehicle.DirtLevel = 0f;

            _notifications?.NotifySuccess("Vehicle Cleaned");

            Logger.Info("Vehicle cleaned.");
        }

        public void Update()
        {
            Vehicle vehicle = Game.Player.Character?.CurrentVehicle;

            if (vehicle == null || !vehicle.Exists())
            {
                RestorePreviousVehicle();
                return;
            }

            if (_lastVehicle != null && _lastVehicle.Exists() &&
                _lastVehicle.Handle != vehicle.Handle)
            {
                RestoreVehicleFlags(_lastVehicle);
            }
            _lastVehicle = vehicle;

            if (_godMode)
            {
                vehicle.IsInvincible = true;
                vehicle.EngineHealth = 1000f;
                vehicle.BodyHealth = 1000f;
                vehicle.PetrolTankHealth = 1000f;
            }
            else
            {
                vehicle.IsInvincible = false;
            }

            if (_autoRepair)
            {
                if (vehicle.EngineHealth < 900f ||
                    vehicle.BodyHealth < 900f)
                {
                    vehicle.Repair();
                }
            }

            if (_invincibleTires)
            {
                Function.Call(
                    Hash.SET_VEHICLE_TYRES_CAN_BURST,
                    vehicle.Handle,
                    false);
            }
            else
            {
                Function.Call(
                    Hash.SET_VEHICLE_TYRES_CAN_BURST,
                    vehicle.Handle,
                    true);
            }
        }

        private void RestorePreviousVehicle()
        {
            if (_lastVehicle != null && _lastVehicle.Exists())
            {
                RestoreVehicleFlags(_lastVehicle);
            }
            _lastVehicle = null;
        }

        private void RestoreVehicleFlags(Vehicle vehicle)
        {
            try
            {
                vehicle.IsInvincible = false;
                Function.Call(Hash.SET_VEHICLE_TYRES_CAN_BURST, vehicle.Handle, true);
            }
            catch (System.Exception ex)
            {
                Logger.Warn("Could not restore vehicle flags: " + ex.Message);
            }
        }
    }
}