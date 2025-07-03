using FarmXpert.Data;
using FarmXpert.Models;
using Microsoft.AspNetCore.SignalR;
using FarmXpert.Hubs;

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
    }
}
