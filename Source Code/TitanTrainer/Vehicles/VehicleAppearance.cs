using System;
using GTA;
using GTA.Native;
using TitanTrainer.Utils;

namespace TitanTrainer.Vehicles
{
    /// <summary>Vehicle paint, lighting, wheels, window tint, smoke and livery customization.</summary>
    public sealed class VehicleAppearance
    {
        private readonly NotificationService _notifications;
        private bool _rainbowPaint;
        private bool _neons;
        private int _rainbowIndex;
        private int _rainbowFrameCounter;

        private readonly VehicleColor[] _colors =
        {
            VehicleColor.MetallicBlack, VehicleColor.MetallicRed, VehicleColor.MetallicBlue,
            VehicleColor.MetallicGreen, VehicleColor.MetallicTaxiYellow, VehicleColor.MetallicOrange,
            VehicleColor.MetallicPurple, VehicleColor.MetallicWhite
        };

        private static readonly int[][] Palette =
        {
            new[] { 245, 245, 245 }, new[] { 18, 18, 22 }, new[] { 210, 24, 35 },
            new[] { 20, 90, 220 }, new[] { 20, 165, 75 }, new[] { 255, 165, 15 },
            new[] { 125, 45, 190 }, new[] { 0, 210, 220 }, new[] { 255, 70, 155 },
            new[] { 120, 120, 130 }, new[] { 245, 210, 40 }, new[] { 90, 45, 25 }
        };

        public VehicleAppearance(NotificationService notifications) { _notifications = notifications; }

        public bool RainbowPaint
        {
            get => _rainbowPaint;
            set { _rainbowPaint = value; _notifications?.NotifyInfo(value ? "Rainbow Paint Enabled" : "Rainbow Paint Disabled"); }
        }

        public bool Neons
        {
            get => _neons;
            set
            {
                _neons = value;
                var vehicle = CurrentVehicle();
                if (vehicle != null)
                    for (int i = 0; i < 4; i++) Function.Call(Hash.SET_VEHICLE_NEON_ENABLED, vehicle.Handle, i, value);
                _notifications?.NotifyInfo(value ? "Neons Enabled" : "Neons Disabled");
            }
        }

        public void SetPrimaryColor(VehicleColor color)
        {
            var vehicle = CurrentVehicle(); if (vehicle == null) return;
            vehicle.Mods.PrimaryColor = color;
            _notifications?.NotifySuccess("Primary paint updated.");
        }

        public void SetSecondaryColor(VehicleColor color)
        {
            var vehicle = CurrentVehicle(); if (vehicle == null) return;
            vehicle.Mods.SecondaryColor = color;
            _notifications?.NotifySuccess("Secondary paint updated.");
        }

        public void ApplyColorPreset(int index)
        {
            var vehicle = CurrentVehicle(); if (vehicle == null) return;
            var rgb = Palette[Math.Abs(index) % Palette.Length];
            try
            {
                Function.Call(Hash.SET_VEHICLE_CUSTOM_PRIMARY_COLOUR, vehicle.Handle, rgb[0], rgb[1], rgb[2]);
                Function.Call(Hash.SET_VEHICLE_CUSTOM_SECONDARY_COLOUR, vehicle.Handle, rgb[0], rgb[1], rgb[2]);
                _notifications?.NotifySuccess("Custom paint preset applied.");
            }
            catch (Exception ex) { LogFailure("apply custom paint", ex); }
        }

        public void ApplyTwoTonePreset(int primaryIndex, int secondaryIndex)
        {
            var vehicle = CurrentVehicle(); if (vehicle == null) return;
            var primary = Palette[Math.Abs(primaryIndex) % Palette.Length];
            var secondary = Palette[Math.Abs(secondaryIndex) % Palette.Length];
            try
            {
                Function.Call(Hash.SET_VEHICLE_CUSTOM_PRIMARY_COLOUR, vehicle.Handle, primary[0], primary[1], primary[2]);
                Function.Call(Hash.SET_VEHICLE_CUSTOM_SECONDARY_COLOUR, vehicle.Handle, secondary[0], secondary[1], secondary[2]);
                _notifications?.NotifySuccess("Two-tone paint applied.");
            }
            catch (Exception ex) { LogFailure("apply two-tone paint", ex); }
        }

        public void ClearCustomPaint()
        {
            var vehicle = CurrentVehicle(); if (vehicle == null) return;
            try
            {
                Function.Call(Hash.CLEAR_VEHICLE_CUSTOM_PRIMARY_COLOUR, vehicle.Handle);
                Function.Call(Hash.CLEAR_VEHICLE_CUSTOM_SECONDARY_COLOUR, vehicle.Handle);
                _notifications?.NotifySuccess("Custom paint cleared.");
            }
            catch (Exception ex) { LogFailure("clear custom paint", ex); }
        }

        public void SetWheelType(int wheelType)
        {
            var vehicle = CurrentVehicle(); if (vehicle == null) return;
            try
            {
                Function.Call(Hash.SET_VEHICLE_WHEEL_TYPE, vehicle.Handle, Math.Max(0, Math.Min(7, wheelType)));
                Function.Call(Hash.SET_VEHICLE_MOD, vehicle.Handle, 23, -1, false);
                Function.Call(Hash.SET_VEHICLE_MOD, vehicle.Handle, 24, -1, false);
                _notifications?.NotifySuccess("Wheel category changed. Open Mod Slots to choose a wheel design.");
            }
            catch (Exception ex) { LogFailure("change wheel type", ex); }
        }

        public void SetWindowTint(int tint)
        {
            var vehicle = CurrentVehicle(); if (vehicle == null) return;
            try
            {
                Function.Call(Hash.SET_VEHICLE_WINDOW_TINT, vehicle.Handle, Math.Max(0, Math.Min(6, tint)));
                _notifications?.NotifySuccess("Window tint updated.");
            }
            catch (Exception ex) { LogFailure("set window tint", ex); }
        }

        public void SetXenon(bool enabled, int color)
        {
            var vehicle = CurrentVehicle(); if (vehicle == null) return;
            try
            {
                Function.Call(Hash.TOGGLE_VEHICLE_MOD, vehicle.Handle, 22, enabled);
                if (enabled) Function.Call((Hash)0xE41033B25D003A07UL, vehicle.Handle, Math.Max(0, Math.Min(12, color)));
                _notifications?.NotifySuccess(enabled ? "Xenon headlights enabled." : "Xenon headlights disabled.");
            }
            catch (Exception ex) { LogFailure("set xenon headlights", ex); }
        }

        public void SetNeonColor(int index)
        {
            var vehicle = CurrentVehicle(); if (vehicle == null) return;
            var rgb = Palette[Math.Abs(index) % Palette.Length];
            try
            {
                for (int i = 0; i < 4; i++) Function.Call(Hash.SET_VEHICLE_NEON_ENABLED, vehicle.Handle, i, true);
                Function.Call(Hash.SET_VEHICLE_NEON_COLOUR, vehicle.Handle, rgb[0], rgb[1], rgb[2]);
                _neons = true;
                _notifications?.NotifySuccess("Neon color updated.");
            }
            catch (Exception ex) { LogFailure("set neon color", ex); }
        }

        public void SetTyreSmokeColor(int index)
        {
            var vehicle = CurrentVehicle(); if (vehicle == null) return;
            var rgb = Palette[Math.Abs(index) % Palette.Length];
            try
            {
                Function.Call(Hash.TOGGLE_VEHICLE_MOD, vehicle.Handle, 20, true);
                Function.Call(Hash.SET_VEHICLE_TYRE_SMOKE_COLOR, vehicle.Handle, rgb[0], rgb[1], rgb[2]);
                _notifications?.NotifySuccess("Tire smoke color updated.");
            }
            catch (Exception ex) { LogFailure("set tire smoke color", ex); }
        }

        public void CycleLivery()
        {
            var vehicle = CurrentVehicle(); if (vehicle == null) return;
            try
            {
                int count = Function.Call<int>(Hash.GET_VEHICLE_LIVERY_COUNT, vehicle.Handle);
                if (count > 0)
                {
                    int current = Function.Call<int>(Hash.GET_VEHICLE_LIVERY, vehicle.Handle);
                    int next = (current + 1) % count;
                    Function.Call(Hash.SET_VEHICLE_LIVERY, vehicle.Handle, next);
                    _notifications?.NotifySuccess("Livery " + (next + 1) + " / " + count);
                    return;
                }
                count = Function.Call<int>(Hash.GET_NUM_VEHICLE_MODS, vehicle.Handle, 48);
                if (count > 0)
                {
                    int current = Function.Call<int>(Hash.GET_VEHICLE_MOD, vehicle.Handle, 48);
                    int next = (current + 1) % count;
                    Function.Call(Hash.SET_VEHICLE_MOD, vehicle.Handle, 48, next, false);
                    _notifications?.NotifySuccess("Vehicle livery mod changed.");
                    return;
                }
                _notifications?.NotifyInfo("This vehicle has no supported liveries.");
            }
            catch (Exception ex) { LogFailure("cycle livery", ex); }
        }

        public void CycleMod(int slot)
        {
            var vehicle = CurrentVehicle(); if (vehicle == null) return;
            try
            {
                Function.Call(Hash.SET_VEHICLE_MOD_KIT, vehicle.Handle, 0);
                int count = Function.Call<int>(Hash.GET_NUM_VEHICLE_MODS, vehicle.Handle, slot);
                if (count <= 0) { _notifications?.NotifyInfo("No mods available for this slot."); return; }
                int current = Function.Call<int>(Hash.GET_VEHICLE_MOD, vehicle.Handle, slot);
                int next = current + 1;
                if (next >= count) next = -1;
                Function.Call(Hash.SET_VEHICLE_MOD, vehicle.Handle, slot, next, false);
                _notifications?.NotifySuccess("Vehicle modification updated.");
            }
            catch (Exception ex) { LogFailure("cycle vehicle mod", ex); }
        }

        public int GetModCount(int slot)
        {
            var vehicle = CurrentVehicle();
            if (vehicle == null) return 0;
            try { Function.Call(Hash.SET_VEHICLE_MOD_KIT, vehicle.Handle, 0); return Function.Call<int>(Hash.GET_NUM_VEHICLE_MODS, vehicle.Handle, slot); }
            catch { return 0; }
        }

        public void Update()
        {
            var vehicle = CurrentVehicle();
            if (vehicle == null || !_rainbowPaint) return;
            _rainbowFrameCounter++;
            if (_rainbowFrameCounter < 10) return;
            _rainbowFrameCounter = 0;
            _rainbowIndex = (_rainbowIndex + 1) % _colors.Length;
            vehicle.Mods.PrimaryColor = _colors[_rainbowIndex];
            vehicle.Mods.SecondaryColor = _colors[_rainbowIndex];
            var neon = Palette[_rainbowIndex % Palette.Length];
            Function.Call(Hash.SET_VEHICLE_NEON_COLOUR, vehicle.Handle, neon[0], neon[1], neon[2]);
        }

        private static Vehicle CurrentVehicle()
        {
            try
            {
                var character = Game.Player.Character;
                if (character == null || !character.Exists() || !character.IsInVehicle()) return null;
                var vehicle = character.CurrentVehicle;
                return vehicle != null && vehicle.Exists() ? vehicle : null;
            }
            catch { return null; }
        }

        private static void LogFailure(string operation, Exception ex)
        {
            TitanTrainer.Core.Logger.Warn("Could not " + operation + ": " + ex.Message);
        }
    }
}
