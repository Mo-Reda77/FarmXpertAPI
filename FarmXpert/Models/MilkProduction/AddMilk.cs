using System.ComponentModel.DataAnnotations;

namespace FarmXpert.Models.MilkProduction
{
    public class AddMilk
    {
        [Required]
        public string TagNumber { get; set; }

        public double? AM { get; set; }
        public double? Noon { get; set; }
        public double? PM { get; set; }

        public string? Notes { get; set; }

    }
}
