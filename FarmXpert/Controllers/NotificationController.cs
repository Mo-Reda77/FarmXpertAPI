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
        [Authorize(Roles = "manager")]
        [HttpPost("custom")]
        public async Task<IActionResult> SendCustomNotification([FromBody] NotificationRequest request)
        {
            // Confirm that the email has been sent
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new { Message = "Email required." });
            }

            // التحقق من وجود المستخدم بناءً على الإيميل
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            var worker = await _context.Workers.FirstOrDefaultAsync(w => w.Email == request.Email);
            var vet = await _context.Veterinarians.FirstOrDefaultAsync(v => v.Email == request.Email);

            if (user == null && worker == null && vet == null)
            {
                return NotFound(new { Message = "No user found based on email." });
            }

            // Create the notification
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

            // Send notification via the service
            await _notificationService.SendNotificationAsync(notification);

            return Ok(new { Message = "Notification has been sent." });
        }

        [Authorize(Roles = "manager,Worker")]
        [HttpGet("worker-notifications")]
        public async Task<IActionResult> GetNotificationsForWorker([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(new { Message = "Email is required." });

            var notifications = await _context.Notifications
                .Where(n => n.Email == email)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return Ok(notifications);
        }
        [Authorize(Roles = "manager")]
        [HttpGet("all-worker-notifications")]
        public async Task<IActionResult> GetAllWorkerNotifications()
        {
            // أولاً، نحصل على كل إيميلات العمال
            var workerEmails = await _context.Workers
                .Select(w => w.Email)
                .ToListAsync();

            // ثم نعرض الإشعارات التي تخص هؤلاء العمال فقط
            var notifications = await _context.Notifications
                .Where(n => workerEmails.Contains(n.Email))
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return Ok(notifications);
        }

        [Authorize(Roles = "manager,Worker")]
        [HttpPut("mark-as-read/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);

            if (notification == null)
                return NotFound(new { Message = "Notification not found." });

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Notification marked as read." });
        }
        [Authorize(Roles = "manager")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
                return NotFound(new { Message = "Notification not found." });

            // التحقق أن المستخدم مدير وصاحب الإشعار
           // if (userRole?.ToLower() != "manager" || notification.Email.ToLower() != userEmail?.ToLower())
           //     return Unauthorized(new { Message = "Only the manager who created the notification can delete it." });

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Notification deleted successfully." });
        }



    }


}
