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
        private int _rainbowFrameCounter;

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

            // Script ticks can run every rendered frame. Throttle changes to avoid
            // rapid flashing and avoid allocating a new Random instance every tick.
            _rainbowFrameCounter++;
            if (_rainbowFrameCounter < 10) return;
            _rainbowFrameCounter = 0;

            _rainbowIndex = (_rainbowIndex + 1) % _colors.Length;
            vehicle.Mods.PrimaryColor = _colors[_rainbowIndex];
            vehicle.Mods.SecondaryColor = _colors[_rainbowIndex];

            // A stable palette keeps neon color synchronized with the paint cycle.
            int[][] neonColors =
            {
                new[] { 255, 0, 0 }, new[] { 255, 96, 0 },
                new[] { 255, 220, 0 }, new[] { 0, 220, 60 },
                new[] { 0, 120, 255 }, new[] { 90, 0, 255 },
                new[] { 220, 0, 255 }, new[] { 255, 255, 255 }
            };
            var neon = neonColors[_rainbowIndex];
            Function.Call(Hash.SET_VEHICLE_NEON_COLOUR, vehicle.Handle,
                neon[0], neon[1], neon[2]);
        }
    }
}