using System.ComponentModel.DataAnnotations;

namespace FarmXpert.Models
{
    public class AddManagerRequest
    {
        [Required(ErrorMessage = "The Name field is required.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "The Email field is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "The Password field is required.")]
        [MinLength(6, ErrorMessage = "The Password must be at least 6 characters long.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "The FarmId field is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid FarmId.")]
        public int FarmId { get; set; }
    }
}
