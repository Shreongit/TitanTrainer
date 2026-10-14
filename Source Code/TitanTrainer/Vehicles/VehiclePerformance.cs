using GTA;
using GTA.Native;
using TitanTrainer.Utils;

namespace TitanTrainer.Vehicles
{
    public sealed class VehiclePerformance
    {
        private readonly NotificationService _notifications;

        private bool _turbo;
        private bool _boost;
        private bool _maxUpgrade;

        private float _gravity = 9.81f;
        private float _torqueMultiplier = 1.0f;

        public VehiclePerformance(NotificationService notifications)
        {
            _notifications = notifications;
        }

        public bool Turbo
        {
            get => _turbo;
            set
            {
                _turbo = value;

                _notifications?.NotifyInfo(
                    value ? "Turbo Enabled"
                          : "Turbo Disabled");
            }
        }

        public bool Boost
        {
            get => _boost;
            set
            {
                _boost = value;

                _notifications?.NotifyInfo(
                    value ? "Boost Enabled"
                          : "Boost Disabled");
            }
        }

        public bool MaxUpgrade
        {
            get => _maxUpgrade;
            set
            {
                _maxUpgrade = value;

                _notifications?.NotifyInfo(
                    value ? "Max Upgrade Enabled"
                          : "Max Upgrade Disabled");
            }
        }

        public float Gravity
        {
            get => _gravity;
            set => _gravity = value;
        }

        public float TorqueMultiplier
        {
            get => _torqueMultiplier;
            set => _torqueMultiplier = value;
        }

        public void UpgradeVehicle()
        {
            Vehicle vehicle = Game.Player.Character.CurrentVehicle;

            if (vehicle == null)
                return;

            Function.Call(Hash.SET_VEHICLE_MOD_KIT, vehicle.Handle, 0);

            for (int i = 0; i < 50; i++)
            {
                int mods = Function.Call<int>(
                    Hash.GET_NUM_VEHICLE_MODS,
                    vehicle.Handle,
                    i);

                if (mods > 0)
                {
                    Function.Call(
                        Hash.SET_VEHICLE_MOD,
                        vehicle.Handle,
                        i,
                        mods - 1,
                        false);
                }
            }

            Function.Call(Hash.TOGGLE_VEHICLE_MOD, vehicle.Handle, 18, true);

            _notifications?.NotifySuccess("Vehicle Fully Upgraded");
        }

        public void Update()
        {
            Vehicle vehicle = Game.Player.Character.CurrentVehicle;

            if (vehicle == null)
                return;

            Function.Call(
                Hash.SET_VEHICLE_CHEAT_POWER_INCREASE,
                vehicle.Handle,
                _boost ? 2.0f : 1.0f);

            Function.Call(
                Hash.SET_VEHICLE_GRAVITY,
                vehicle.Handle,
                _gravity);

            float power = _boost ? 2.0f : _torqueMultiplier;

            Function.Call(
                Hash.SET_VEHICLE_CHEAT_POWER_INCREASE,
                vehicle.Handle,
                power);

            if (_turbo)
            {
                Function.Call(Hash.TOGGLE_VEHICLE_MOD, vehicle.Handle, 18, true);
            }

            if (_maxUpgrade)
            {
                UpgradeVehicle();
                _maxUpgrade = false;
            }
        }
    }
}