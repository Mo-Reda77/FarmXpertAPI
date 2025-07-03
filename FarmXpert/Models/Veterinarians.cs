using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FarmXpert.Models
{
    public class Veterinarians
    {
        public int Id { get; set; }

        public string Code { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 4);

        public string? ImagePath { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Specialty { get; set; }

        [Required, Phone]
        public string Phone { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Salary { get; set; }

        [Required]
        [MaxLength(14)]
        public string NationalID { get; set; }

        public string? Age { get; set; }

        public string? Experience { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required, EmailAddress]

        public string Email { get; set; }


        public string PasswordHash { get; set; }

        public string Role { get; set; } = "Veterin";


        public int FarmId { get; set; }




        public string? ResetCode { get; set; }
        public DateTime? ResetCodeExpires { get; set; }



        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Email);
        }
    }
}

