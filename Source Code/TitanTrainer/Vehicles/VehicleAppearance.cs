using System;
using GTA;
using GTA.Native;
using TitanTrainer.Utils;

namespace TitanTrainer.Vehicles
{
    public sealed class VehicleAppearance
    {
        private readonly NotificationService _notifications;

        private bool _rainbowPaint;
        private bool _neons;

        private int _rainbowIndex;

        private readonly VehicleColor[] _colors =
        {
            VehicleColor.MetallicBlack,
            VehicleColor.MetallicRed,
            VehicleColor.MetallicBlue,
            VehicleColor.MetallicGreen,
            VehicleColor.MetallicTaxiYellow,
            VehicleColor.MetallicOrange,
            VehicleColor.MetallicPurple,
            VehicleColor.MetallicWhite
        };

        public VehicleAppearance(NotificationService notifications)
        {
            _notifications = notifications;
        }

        public bool RainbowPaint
        {
            get => _rainbowPaint;
            set
            {
                _rainbowPaint = value;

                _notifications?.NotifyInfo(
                    value
                        ? "Rainbow Paint Enabled"
                        : "Rainbow Paint Disabled");
            }
        }

        public bool Neons
        {
            get => _neons;
            set
            {
                _neons = value;

                Vehicle vehicle = Game.Player.Character.CurrentVehicle;

                if (vehicle == null)
                    return;

                for (int i = 0; i < 4; i++)
                {
                    Function.Call(
                        Hash.SET_VEHICLE_NEON_ENABLED,
                        vehicle.Handle,
                        i,
                        value);
                }

                _notifications?.NotifyInfo(
                    value
                        ? "Neons Enabled"
                        : "Neons Disabled");
            }
        }

        public void SetPrimaryColor(VehicleColor color)
        {
            Vehicle vehicle = Game.Player.Character.CurrentVehicle;

            if (vehicle == null)
                return;

            vehicle.Mods.PrimaryColor = color;
        }

        public void SetSecondaryColor(VehicleColor color)
        {
            Vehicle vehicle = Game.Player.Character.CurrentVehicle;

            if (vehicle == null)
                return;

            vehicle.Mods.SecondaryColor = color;
        }

        public void Update()
        {
            Vehicle vehicle = Game.Player.Character.CurrentVehicle;

            if (vehicle == null)
                return;

            if (!_rainbowPaint)
                return;

            _rainbowIndex++;

            if (_rainbowIndex >= _colors.Length)
                _rainbowIndex = 0;

            vehicle.Mods.PrimaryColor = _colors[_rainbowIndex];
            vehicle.Mods.SecondaryColor = _colors[_rainbowIndex];

            Random random = new Random();

            Function.Call(
                Hash.SET_VEHICLE_NEON_COLOUR,
                vehicle.Handle,
                random.Next(256),
                random.Next(256),
                random.Next(256));
        }
    }
}