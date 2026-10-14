using System;
using GTA;
using GTA.Native;
using TitanTrainer.Core;
using TitanTrainer.Utils;

namespace TitanTrainer.World
{
    /// <summary>
    /// Handles all in-game time operations.
    /// </summary>
    public sealed class TimeManager : IDisposable
    {
        private readonly NotificationService _notifications;

        private bool _freezeTime;

        private int _hour;
        private int _minute;

        public TimeManager(NotificationService notifications)
        {
            _notifications = notifications;

            _hour = Function.Call<int>(Hash.GET_CLOCK_HOURS);
            _minute = Function.Call<int>(Hash.GET_CLOCK_MINUTES);
        }

        public bool IsFrozen => _freezeTime;

        public int CurrentHour =>
            Function.Call<int>(Hash.GET_CLOCK_HOURS);

        public int CurrentMinute =>
            Function.Call<int>(Hash.GET_CLOCK_MINUTES);

        public void SetTime(int hour, int minute)
        {
            hour = Math.Max(0, Math.Min(23, hour));
            minute = Math.Max(0, Math.Min(59, minute));

            _hour = hour;
            _minute = minute;

            Function.Call(Hash.SET_CLOCK_TIME, hour, minute, 0);

            Logger.Info($"Time changed to {hour:D2}:{minute:D2}");

            _notifications?.NotifySuccess(
                $"Time: {hour:D2}:{minute:D2}");
        }

        public void SetHour(int hour)
        {
            SetTime(hour, CurrentMinute);
        }

        public void AddHour()
        {
            int hour = (CurrentHour + 1) % 24;

            SetTime(hour, CurrentMinute);
        }

        public void RemoveHour()
        {
            int hour = CurrentHour - 1;

            if (hour < 0)
                hour = 23;

            SetTime(hour, CurrentMinute);
        }

        public void AddMinute()
        {
            int minute = CurrentMinute + 30;
            int hour = CurrentHour;

            if (minute >= 60)
            {
                minute -= 60;
                hour = (hour + 1) % 24;
            }

            SetTime(hour, minute);
        }

        public void RemoveMinute()
        {
            int minute = CurrentMinute - 30;
            int hour = CurrentHour;

            if (minute < 0)
            {
                minute += 60;
                hour--;

                if (hour < 0)
                    hour = 23;
            }

            SetTime(hour, minute);
        }

        public void FreezeTime(bool freeze)
        {
            _freezeTime = freeze;

            if (freeze)
            {
                _hour = CurrentHour;
                _minute = CurrentMinute;
            }

            _notifications?.NotifyInfo(
                freeze
                    ? "Time Frozen"
                    : "Time Unfrozen");
        }

        public void Update()
        {
            if (!_freezeTime)
                return;

            Function.Call(
                Hash.SET_CLOCK_TIME,
                _hour,
                _minute,
                0);
        }

        public void Dispose()
        {
            _freezeTime = false;
        }
    }
}