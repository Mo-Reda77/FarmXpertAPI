namespace FarmXpert.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty; // معرف المستخدم
        public string Name { get; set; } = string.Empty;  // اسم المستخدم
        public string Email { get; set; } = string.Empty; // البريد الإلكتروني
        public string Title { get; set; } = string.Empty;   // مثل: "تحذير طقس"
        public string Message { get; set; } = string.Empty; // الرسالة نفسها
        public bool IsRead { get; set; } = false;           // هل تمت قراءة الإشعار؟
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
