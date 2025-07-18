using System.ComponentModel.DataAnnotations;

namespace FarmXpert.Models
{
    public class NotificationRequest
    {
        [Required]
        public string Email { get; set; } // Email to identify the user
        [Required]
        public string Title { get; set; } = string.Empty;
        [Required]
        public string Message { get; set; } = string.Empty;
    }
}
