using System.ComponentModel.DataAnnotations;

namespace FarmXpert.Models
{
    public class ForgetPassword
    {
        [Required(ErrorMessage = "The Email field is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string Email { get; set; }
    }
}
