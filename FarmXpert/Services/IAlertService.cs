using FarmXpert.Models;
namespace FarmXpert.Services
{
    public interface IAlertService
    {
        Task StoreAlertAsync(Alert alert);

        //New
        Task<List<Alert>> GetAllAlertsAsync();


        Task<Alert?> GetAlertByCityAsync(string city);
        Task RemoveAlertByCityAsync(string city);
    }
}
