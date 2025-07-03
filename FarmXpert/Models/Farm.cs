using FarmXpert.Models;
using FarmXpert.Products;
using System.Collections.Generic;

namespace FarmXpert.Models
{
    public class Farm
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<Worker> Workers { get; set; } = new List<Worker>();
        public ICollection<Veterinarians> Veterinarians { get; set; } = new List<Veterinarians>();
        public ICollection<Cattle> Cattle { get; set; } = new List<Cattle>();
    }
}
