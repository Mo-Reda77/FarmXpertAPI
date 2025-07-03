using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FarmXpert.Models;
using FarmXpert.Services;
using FarmXpert.Data;
using Microsoft.EntityFrameworkCore;

namespace FarmXpert.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationController> _logger;
        private readonly FarmDbContext _context;

        public NotificationController(INotificationService notificationService, ILogger<NotificationController> logger, FarmDbContext context)
        {
            _notificationService = notificationService;
            _logger = logger;
            _context = context;
        }

        [HttpPost("custom")]
        public async Task<IActionResult> SendCustomNotification([FromBody] NotificationRequest request)
        {
            // التأكد من إرسال الإيميل
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new { Message = "البريد الإلكتروني مطلوب." });
            }

            // التحقق من وجود المستخدم بناءً على الإيميل
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            var worker = await _context.Workers.FirstOrDefaultAsync(w => w.Email == request.Email);
            var vet = await _context.Veterinarians.FirstOrDefaultAsync(v => v.Email == request.Email);

            if (user == null && worker == null && vet == null)
            {
                return NotFound(new { Message = "لم يتم العثور على مستخدم بناءً على البريد الإلكتروني." });
            }

            // إنشاء الإشعار
            var notification = new Notification
            {
                Email = request.Email,
                Title = request.Title,
                Message = request.Message
            };

            if (worker != null)
            {
                notification.Name = worker.Name;
                notification.UserId = worker.Id.ToString();
            }
            else if (vet != null)
            {
                notification.Name = vet.Name;
                notification.UserId = vet.Id.ToString();
            }
            else if (user != null)
            {
                notification.Name = user.Name;
                notification.UserId = user.Id.ToString();
            }

            // إرسال الإشعار عبر الخدمة
            await _notificationService.SendNotificationAsync(notification);

            return Ok(new { Message = "تم إرسال الإشعار." });
        }
    }
}
