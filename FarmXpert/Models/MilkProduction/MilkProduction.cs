using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmXpert.Models.MilkProduction
{
    public class MilkProduction
    {
        public int Id { get; set; }

        [Required]
        public string TagNumber { get; set; }

        public string CountNumber { get; set; }


        public double AM { get; set; }
        public double Noon { get; set; }
        public double PM { get; set; }

        public double Total { get; set; }

        public string? Notes { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public int FarmID { get; set; }

        // ✅ Optional: ربط بكيان المزرعة
        [ForeignKey("FarmID")]
        public FarmXpert.Models.Farm? Farm { get; set; }
    }
}
