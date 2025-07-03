using Microsoft.AspNetCore.SignalR;
namespace FarmXpert.Hubs
{
    public class NotificationHub : Hub
    {
        // سيقوم المستخدم بالانضمام إلى مجموعة بناءً على دوره
        public async Task JoinGroup(string role)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, role);
        }

        // إرسال إشعار للمجموعة المحددة بناءً على الدور
        //public async Task SendNotificationToRole(string role, string message)
        //{
           // await Clients.Group(role).SendAsync("ReceiveNotification", message);
        //}

    }
}
