using Newtonsoft.Json;

namespace TitanTrainer.Vehicles
{
    /// <summary>
    /// Vehicle data model used by the vehicle database.
    /// </summary>
    public sealed class VehicleDefinition
    {
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("modelName")]
        public string ModelName { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("hash")]
        public uint? Hash { get; set; }

        [JsonProperty("isDlc")]
        public bool IsDlc { get; set; }

        [JsonProperty("notes")]
        public string Notes { get; set; }
    }
}