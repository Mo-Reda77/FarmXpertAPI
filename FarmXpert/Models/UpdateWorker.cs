using System.ComponentModel.DataAnnotations;

namespace FarmXpert.Models
{
    public class UpdateWorker
    {
        
      
      
      

        public string? Name { get; set; }

        public string? Specialty { get; set; }

        public string? Phone { get; set; }
        public string? Age { get; set; }

        public decimal? Salary { get; set; }

        public string? Experience { get; set; }

        public string? NationalID { get; set; }

        public IFormFile? ImagePath { get; set; }
      

    }
}
