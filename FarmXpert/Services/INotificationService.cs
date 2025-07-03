using FarmXpert.Models;
namespace FarmXpert.Services
{
    public interface INotificationService
    {
        Task SendNotificationAsync(Notification notification);
    }
}
