using GTA;
using GTA.Native;
using TitanTrainer.Utils;

namespace TitanTrainer.Vehicles
{
    public sealed class VehicleControls
    {
        private readonly NotificationService _notifications;

        private bool _engine;
        private bool _freezeVehicle;
        private bool _seatbelt;

        public VehicleControls(NotificationService notifications)
        {
            _notifications = notifications;
        }

        public bool Engine
        {
            get => _engine;
            set
            {
                _engine = value;

                Vehicle vehicle = Game.Player.Character.CurrentVehicle;

                if (vehicle != null)
                {
                    vehicle.IsEngineRunning = value;
                }

                _notifications?.NotifyInfo(
                    value
                        ? "Engine On"
                        : "Engine Off");
            }
        }

        public bool FreezeVehicle
        {
            get => _freezeVehicle;
            set
            {
                _freezeVehicle = value;

                Vehicle vehicle = Game.Player.Character.CurrentVehicle;

                if (vehicle != null)
                {
                    vehicle.IsPositionFrozen = value;
                }

                _notifications?.NotifyInfo(
                    value
                        ? "Vehicle Frozen"
                        : "Vehicle Unfrozen");
            }
        }

        public bool Seatbelt
        {
            get => _seatbelt;
            set
            {
                _seatbelt = value;

                _notifications?.NotifyInfo(
                    value
                        ? "Seatbelt Enabled"
                        : "Seatbelt Disabled");
            }
        }

        public void ToggleDoor(int index)
        {
            Vehicle vehicle = Game.Player.Character.CurrentVehicle;

            if (vehicle == null)
                return;

            if (Function.Call<float>(
                    Hash.GET_VEHICLE_DOOR_ANGLE_RATIO,
                    vehicle.Handle,
                    index) > 0.1f)
            {
                Function.Call(
                    Hash.SET_VEHICLE_DOOR_SHUT,
                    vehicle.Handle,
                    index,
                    false);
            }
            else
            {
                Function.Call(
                    Hash.SET_VEHICLE_DOOR_OPEN,
                    vehicle.Handle,
                    index,
                    false,
                    false);
            }
        }

        public void RollDownWindow(int index)
        {
            Vehicle vehicle = Game.Player.Character.CurrentVehicle;

            if (vehicle == null)
                return;

            Function.Call(
                Hash.ROLL_DOWN_WINDOW,
                vehicle.Handle,
                index);
        }

        public void RollUpWindow(int index)
        {
            Vehicle vehicle = Game.Player.Character.CurrentVehicle;

            if (vehicle == null)
                return;

            Function.Call(
                Hash.ROLL_UP_WINDOW,
                vehicle.Handle,
                index);
        }

        public void Update()
        {
            Vehicle vehicle = Game.Player.Character.CurrentVehicle;

            if (vehicle == null)
                return;


            if (_seatbelt)
            {
                Game.Player.Character.CanFlyThroughWindscreen = false;
            }
            else
            {
                Game.Player.Character.CanFlyThroughWindscreen = true;
            }
        }
    }
}