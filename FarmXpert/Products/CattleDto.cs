using System.ComponentModel.DataAnnotations;

namespace FarmXpert.Products
{
    public class CattleDto
    {
        [Required]
        public string Type { get; set; }

        public int? Weight { get; set; }

        
        public string? Gender { get; set; }

        
        public int? Age { get; set; }
    }
}
