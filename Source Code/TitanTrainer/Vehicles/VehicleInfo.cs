namespace TitanTrainer.Vehicles
{
    public sealed class VehicleInfo
    {
        public string DisplayName { get; set; }

        public string ModelName { get; set; }

        public string Manufacturer { get; set; }

        public VehicleCategory Category { get; set; }

        public uint Hash { get; set; }

        public bool IsDlc { get; set; }

        public int Seats { get; set; }
    }
}