using GTA;
using GTA.Math;
using GTA.Native;
using TitanTrainer.Core;
using TitanTrainer.Utils;

namespace TitanTrainer.Vehicles
{
    public sealed class VehicleUtility
    {
        private readonly NotificationService _notifications;

        public VehicleUtility(NotificationService notifications)
        {
            _notifications = notifications;
        }

        public void FlipVehicle()
        {
            Vehicle vehicle = Game.Player.Character.CurrentVehicle;

            if (vehicle == null)
                return;

            vehicle.Rotation = new Vector3(
                0f,
                0f,
                vehicle.Heading);

            vehicle.Velocity = Vector3.Zero;

            _notifications?.NotifySuccess("Vehicle Flipped");

            Logger.Info("Vehicle flipped.");
        }

        public void DeleteVehicle()
        {
            Vehicle vehicle = Game.Player.Character.CurrentVehicle;

            if (vehicle == null)
                return;

            vehicle.Delete();

            _notifications?.NotifySuccess("Vehicle Deleted");

            Logger.Info("Vehicle deleted.");
        }

        public void CloneVehicle()
        {
            Vehicle vehicle = Game.Player.Character.CurrentVehicle;

            if (vehicle == null)
                return;

            Vector3 spawn = vehicle.Position + vehicle.RightVector * 5f;

            Vehicle clone = Vehicle.Create(
                vehicle.Model,
                spawn,
                vehicle.Heading);

            if (clone == null)
                return;

            clone.Mods.InstallModKit();

            clone.Mods.PrimaryColor = vehicle.Mods.PrimaryColor;
            clone.Mods.SecondaryColor = vehicle.Mods.SecondaryColor;

            clone.EngineHealth = vehicle.EngineHealth;
            clone.BodyHealth = vehicle.BodyHealth;
            clone.PetrolTankHealth = vehicle.PetrolTankHealth;

            clone.DirtLevel = vehicle.DirtLevel;

            _notifications?.NotifySuccess("Vehicle Cloned");

            Logger.Info("Vehicle cloned.");
        }

        public void SpawnPersonalVehicle()
        {
            Ped ped = Game.Player.Character;

            if (!ped.IsInVehicle())
                return;

            Vehicle current = ped.CurrentVehicle;

            Model model = current.Model;

            Vector3 spawn = ped.Position + ped.ForwardVector * 6f;

            Vehicle vehicle = Vehicle.Create(
                model,
                spawn,
                ped.Heading);

            if (vehicle == null)
                return;

            vehicle.PlaceOnGround();

            _notifications?.NotifySuccess("Personal Vehicle Spawned");

            Logger.Info("Personal vehicle spawned.");
        }

        public void WarpIntoVehicle()
        {
            Vehicle vehicle = Game.Player.Character.LastVehicle;

            if (vehicle == null || !vehicle.Exists())
                return;

            Game.Player.Character.SetIntoVehicle(
                vehicle,
                VehicleSeat.Driver);

            _notifications?.NotifySuccess("Warped Into Vehicle");
        }

        public void ExplodeVehicle()
        {
            Vehicle vehicle = Game.Player.Character.CurrentVehicle;

            if (vehicle == null)
                return;

            Function.Call(
                Hash.EXPLODE_VEHICLE,
                vehicle.Handle,
                true,
                false);

            _notifications?.NotifyInfo("Boom 💥");
        }
    }
}