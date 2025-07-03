using FarmXpert.Models;

using System.ComponentModel.DataAnnotations.Schema;

namespace FarmXpert.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; } // "Manager" أو "Worker"
        public int FarmId { get; set; }
        public Farm Farm { get; set; }
        public string? ResetCode { get; set; }
        public DateTime? ResetCodeExpires { get; set; }

        // هذه الخاصية غير مخزنة في قاعدة البيانات
        [NotMapped]
        public string PasswordPlain { get; set; }
    }
}

