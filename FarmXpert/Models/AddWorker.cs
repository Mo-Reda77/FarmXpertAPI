using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmXpert.Models
{
    public class AddWorker
    {
        
        public IFormFile? Image { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
       
        public string NationalID { get; set; }
        [Required]
        public string Specialty { get; set; }
     
        public string? Age { get; set; }
        [Required ,EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required, Phone]
        public String Phone { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public int? Salary { get;  set; }
        public string? Experience { get; set; }
    }
}
