namespace TitanTrainer.Vehicles
{
    public static class ManufacturerMapper
    {
        public static string GetManufacturer(string model)
        {
            switch (model.ToLower())
            {
                case "adder":
                case "nero":
                case "entityxf":
                case "entitymt":
                    return "Truffade";

                case "zentorno":
                case "ignus":
                case "tempesta":
                case "toreroxo":
                case "tezeract":
                    return "Pegassi";

                case "t20":
                case "fmj":
                case "italigto":
                case "italirsx":
                case "furia":
                    return "Grotti";

                case "krieger":
                case "schlagen":
                case "feltzer":
                    return "Benefactor";

                case "osiris":
                    return "Pegassi";

                case "emerus":
                    return "Progen";

                case "xa21":
                    return "Ocelot";

                case "vagner":
                    return "Dewbauchee";

                default:
                    return "Unknown";
            }
        }
    }
}