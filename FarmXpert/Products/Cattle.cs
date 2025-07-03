using FarmXpert.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmXpert.Products
{
    public class Cattle
    {
        [Key]
        public int CattleID { get; set; }
        [Required]
        public string Type { get; set; }  

        
        public int? Weight { get; set; } 

       
        public string? Gender { get; set; } 

        public int? Age { get; set; } 

        [Required]
        public int FarmID { get; set; }  

        [ForeignKey("FarmID")]
        public Farm Farm { get; set; } 
    }
}
