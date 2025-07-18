namespace FarmXpert.Models.MilkProduction
{
    public class AddMilkMultiple
    {
        public string CountNumber { get; set; } = "";


        public double? AM { get; set; }
        public double? Noon { get; set; }
        public double? PM { get; set; }

        public string? Notes { get; set; }
    }
}
