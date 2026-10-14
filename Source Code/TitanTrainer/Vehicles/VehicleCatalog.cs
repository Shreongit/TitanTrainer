using GTA;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TitanTrainer.Vehicles
{
    public sealed class VehicleCatalog
    {
        private readonly List<VehicleInfo> _vehicles = new List<VehicleInfo>();

        public IReadOnlyList<VehicleInfo> Vehicles => _vehicles;

        public VehicleCatalog()
        {
            Load();
        }

        private void Load()
        {
            _vehicles.Clear();

            foreach (VehicleHash hash in Enum.GetValues(typeof(VehicleHash)))
            {
                string model = hash.ToString().ToLower();

                _vehicles.Add(new VehicleInfo
                {
                    Hash = (uint)hash,
                    ModelName = model,
                    DisplayName = Beautify(model),
                    Manufacturer = ManufacturerMapper.GetManufacturer(model),
                    Category = VehicleCategoryMapper.GetCategory(model),
                    IsDlc = false,
                    Seats = 0
                });
            }

            _vehicles.Sort((a, b) =>
                string.Compare(a.DisplayName, b.DisplayName, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<VehicleInfo> GetByCategory(VehicleCategory category)
        {
            return _vehicles.Where(v => v.Category == category);
        }

        public IEnumerable<VehicleInfo> Search(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return _vehicles;

            text = text.ToLower();

            return _vehicles.Where(v =>
                v.DisplayName.ToLower().Contains(text) ||
                v.ModelName.ToLower().Contains(text) ||
                v.Manufacturer.ToLower().Contains(text));
        }

        private static string Beautify(string model)
        {
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo
                .ToTitleCase(model.Replace("_", " "));
        }
    }
}