namespace FarmXpert.Models.Client
{
    public class ClientRequest
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FarmName { get; set; }

        public string PhoneNumber { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
