using System.ComponentModel.DataAnnotations;

namespace FarmXpert.Models
{
    public class Update_Manager
    {
        
        public string? Name { get; set; } = string.Empty;
        
        public string? Email { get; set; } = string.Empty;
        
        public string? Password { get; set; } = string.Empty;
        
        public int? FarmId { get; set; }
    }
}
