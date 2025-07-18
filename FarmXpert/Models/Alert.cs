namespace FarmXpert.Models
{
    public class Alert
    {
        public int Id { get; set; }
        public string City { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
