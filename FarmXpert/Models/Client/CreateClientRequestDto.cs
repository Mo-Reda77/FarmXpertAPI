using System.ComponentModel.DataAnnotations;

namespace FarmXpert.Models.Client
{
    public class CreateClientRequestDto
    {

        [Required(ErrorMessage = "The Email field is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "The FarmName field is required.")]
        public string FarmName { get; set; }


        [Required(ErrorMessage = "The Phone field is required.")]
        public string PhoneNumber { get; set; }

     
    }
}
