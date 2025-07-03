using FarmXpert.Models;

namespace FarmXpert.Services
{
    public interface IWeatherService
    {
        Task<WeatherResponse?> GetWeatherAsync(string city);
        Task<WeatherResponse?> GetWeatherByCoordinatesAsync(double latitude, double longitude);
        Task<string?> GetCityNameByCoordinatesAsync(double lat, double lon); // إضافة هذه السطر
    }
}
