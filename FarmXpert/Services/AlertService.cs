using FarmXpert.Data;
using FarmXpert.Models;
using Microsoft.AspNetCore.SignalR;
using FarmXpert.Hubs;
using Microsoft.EntityFrameworkCore;

namespace FarmXpert.Services
{
    public class AlertService : IAlertService
    {
        private readonly FarmDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public AlertService(FarmDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // تخزين التنبيه في قاعدة البيانات وإرسال إشعار فوري
        public async Task StoreAlertAsync(Alert alert)
        {
            _context.Alerts.Add(alert);
            await _context.SaveChangesAsync();


            // إرسال الإشعار الفوري
            await _hubContext.Clients.All.SendAsync("ReceiveAlert", new
            {
                alert.City,
                alert.Message,
                alert.CreatedAt
            });

        }
        ///New
        public async Task<List<Alert>> GetAllAlertsAsync()
        {
            return await _context.Alerts
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        //  جديد: جلب التنبيه لمدينة معينة
        public async Task<Alert?> GetAlertByCityAsync(string city)
        {
            return await _context.Alerts.FirstOrDefaultAsync(a => a.City == city);
        }

        //  جديد: حذف التنبيه عند عودة الطقس طبيعي
        public async Task RemoveAlertByCityAsync(string city)
        {
            var alert = await _context.Alerts.FirstOrDefaultAsync(a => a.City == city);
            if (alert != null)
            {
                _context.Alerts.Remove(alert);
                await _context.SaveChangesAsync();

                await _hubContext.Clients.All.SendAsync("AlertRemoved", new { City = city });
            }
        }
    }
}
