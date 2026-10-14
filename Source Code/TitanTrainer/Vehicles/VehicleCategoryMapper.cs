namespace TitanTrainer.Vehicles
{
    public static class VehicleCategoryMapper
    {
        public static VehicleCategory GetCategory(string model)
        {
            switch (model.ToLower())
            {
                // ===== SUPER =====
                case "adder":
                case "zentorno":
                case "t20":
                case "osiris":
                case "krieger":
                case "ignus":
                case "entityxf":
                case "entitymt":
                case "tempesta":
                case "turismor":
                case "cheetah":
                case "furia":
                case "vagner":
                case "xa21":
                case "emerus":
                case "deveste":
                case "cyclone":
                case "cyclone2":
                case "tyrant":
                case "torero2":
                case "lm87":
                case "visione":
                case "bullet":
                case "s80":
                    return VehicleCategory.Super;

                // ===== SPORTS =====
                case "pariah":
                case "kuruma":
                case "elegy2":
                case "jester":
                case "italigto":
                case "italirsx":
                case "comet2":
                case "comet5":
                case "sultan":
                case "schlagen":
                case "banshee":
                case "buffalo2":
                    return VehicleCategory.Sports;

                // ===== MUSCLE =====
                case "dominator":
                case "dominator7":
                case "dominator8":
                case "gauntlet":
                case "gauntlet4":
                case "dukes":
                case "vigero2":
                case "sabregt":
                case "buffalo4":
                    return VehicleCategory.Muscle;

                // ===== MOTORCYCLE =====
                case "bati":
                case "bati2":
                case "hakuchou":
                case "akuma":
                    return VehicleCategory.Motorcycle;

                // ===== PLANE =====
                case "hydra":
                case "lazer":
                case "raiju":
                case "luxor":
                    return VehicleCategory.Plane;

                // ===== HELICOPTER =====
                case "buzzard2":
                case "akula":
                case "hunter":
                    return VehicleCategory.Helicopter;

                // ===== BOAT =====
                case "jetmax":
                case "longfin":
                case "speeder":
                    return VehicleCategory.Boat;

                // ===== VAN =====
                case "bison":
                case "speedo":
                case "pony":
                case "burrito3":
                case "minivan":
                    return VehicleCategory.Van;

                default:
                    return VehicleCategory.Unknown;
            }
        }
    }
}