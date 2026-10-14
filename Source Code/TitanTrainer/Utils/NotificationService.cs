using System;
using GTA;
using TitanTrainer.Core;

namespace TitanTrainer.Utils
{
    /// <summary>
    /// Lightweight notification wrapper for showing messages to the player.
    /// Uses GTA.UI.Screen.ShowSubtitle to avoid obsolete Notification.Show usage.
    /// </summary>
    public sealed class NotificationService
    {
        public void NotifyInfo(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            try
            {
                // Use subtitle to show messages without using obsolete Notification.Show
                GTA.UI.Screen.ShowSubtitle(message, 3000);
            }
            catch (Exception ex)
            {
                Logger.Warn("NotifyInfo failed: " + ex);
            }
        }

        public void NotifySuccess(string message) => NotifyInfo(message);

        public void NotifyError(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            try
            {
                GTA.UI.Screen.ShowSubtitle("ERROR: " + message, 5000);
            }
            catch (Exception ex)
            {
                Logger.Warn("NotifyError failed: " + ex);
            }
        }
    }
}