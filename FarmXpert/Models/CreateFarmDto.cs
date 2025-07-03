using System.ComponentModel.DataAnnotations;

namespace FarmXpert.Models
{
    public class CreateFarmDto
    {
        [Required]
        public string Name { get; set; }
    }
}
