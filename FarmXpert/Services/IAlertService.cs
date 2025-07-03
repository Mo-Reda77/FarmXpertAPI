using FarmXpert.Models;
namespace FarmXpert.Services
{
    public interface IAlertService
    {
        Task StoreAlertAsync(Alert alert);
    }
}
