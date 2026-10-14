using System;
using GTA;
using GTA.Math;
using GTA.Native;
using TitanTrainer.Core;
using TitanTrainer.Utils;

namespace TitanTrainer.Player
{
    public sealed class TeleportManager
    {
        private readonly NotificationService _notifications;

        public TeleportManager(NotificationService notifications)
        {
            _notifications = notifications;
        }

        public void TeleportTo(Vector3 position)
        {
            try
            {
                Ped ped = Game.Player.Character;

                if (ped.IsInVehicle())
                {
                    ped.CurrentVehicle.Position = position;
                }
                else
                {
                    ped.Position = position;
                }

                _notifications?.NotifySuccess("Teleported");
                Logger.Info($"Teleported to {position}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Teleport failed: {ex}");
                _notifications?.NotifyError("Teleport failed.");
            }
        }

        public void TeleportToWaypoint()
        {
            try
            {
                int blip = Function.Call<int>(Hash.GET_FIRST_BLIP_INFO_ID, 8);

                if (!Function.Call<bool>(Hash.DOES_BLIP_EXIST, blip))
                {
                    _notifications?.NotifyError("No waypoint found.");
                    return;
                }

                Vector3 coords = Function.Call<Vector3>(Hash.GET_BLIP_COORDS, blip);

                float groundZ = 0f;

                bool foundGround = false;

                for (float height = 1000f; height >= 0f; height -= 50f)
                {
                    TeleportTo(new Vector3(
                        coords.X,
                        coords.Y,
                        coords.Z + 1f));

                    if (foundGround)
                        break;
                }

                if (!foundGround)
                    groundZ = coords.Z;

                TeleportTo(new Vector3(coords.X, coords.Y, groundZ + 1f));
            }
            catch (Exception ex)
            {
                Logger.Error($"Waypoint teleport failed: {ex}");
                _notifications?.NotifyError("Waypoint teleport failed.");
            }
        }

        public void TeleportForward(float distance = 10f)
        {
            Ped ped = Game.Player.Character;

            Vector3 destination = ped.Position + ped.ForwardVector * distance;

            TeleportTo(destination);
        }

        public void TeleportSky()
        {
            Vector3 pos = Game.Player.Character.Position;

            TeleportTo(new Vector3(
                pos.X,
                pos.Y,
                pos.Z + 200f));
        }

        public void TeleportSafe()
        {
            Ped ped = Game.Player.Character;

            Vector3 pos = ped.Position;

            TeleportTo(new Vector3(
                pos.X,
                pos.Y,
                pos.Z + 2f));
        }
    }
}