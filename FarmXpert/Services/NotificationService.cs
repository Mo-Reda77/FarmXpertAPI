using FarmXpert.Data;
using FarmXpert.Models;
using FarmXpert.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace FarmXpert.Services
{
    public class NotificationService : INotificationService
    {
        private readonly FarmDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(FarmDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task SendNotificationAsync(Notification notification)
        {
            // إضافة الإشعار إلى قاعدة البيانات
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // البحث في جدول Users باستخدام البريد الإلكتروني
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == notification.Email);

            // إذا لم نجد المستخدم في جدول Users، نبحث في جدول Workers
            if (user == null)
            {
                var worker = await _context.Workers.FirstOrDefaultAsync(w => w.Email == notification.Email);
                if (worker != null)
                {
                    user = new User { Id = worker.Id, Email = worker.Email, Name = worker.Name, Role = "Worker" };
                    notification.Name = worker.Name;
                    notification.UserId = worker.Id.ToString();
                    Console.WriteLine("User found in Workers table.");
                }
            }

            // إذا لم نجد المستخدم في جدول Workers، نبحث في جدول Veterinarians
            if (user == null)
            {
                var vet = await _context.Veterinarians.FirstOrDefaultAsync(v => v.Email == notification.Email);
                if (vet != null)
                {
                    user = new User { Id = vet.Id, Email = vet.Email, Name = vet.Name, Role = "Veterin" };
                    notification.Name = vet.Name;
                    notification.UserId = vet.Id.ToString();
                    Console.WriteLine("User found in Veterinarians table.");
                }
            }

            // إذا لم نجد أي مستخدم في الجداول الثلاثة
            if (user == null)
            {
                Console.WriteLine($"No user found with email: {notification.Email}");
                throw new Exception("المستخدم غير موجود");
            }

            // إرسال الإشعار باستخدام SignalR للمجموعة المناسبة حسب الدور
            await _hubContext.Clients.Group(user.Role).SendAsync("ReceiveNotification", new
            {
                notification.UserId,
                notification.Name,
                notification.Email,
                notification.Title,
                notification.Message,
                notification.CreatedAt
            });
        }
    }
}
