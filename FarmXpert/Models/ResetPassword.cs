using System.ComponentModel.DataAnnotations;

namespace FarmXpert.Models
{
    public class ResetPassword
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Code { get; set; }
        [Required]
        public string NewPassword { get; set; }
    }
}
