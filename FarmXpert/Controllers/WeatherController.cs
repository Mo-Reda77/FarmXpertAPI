using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FarmXpert.Models;
using FarmXpert.Services;

namespace FarmXpert.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]

    public class WeatherController : ControllerBase
    {
        private readonly IWeatherService _weatherService;
        private readonly IAlertService _alertService;
        private readonly ILogger<WeatherController> _logger;

        public WeatherController(IWeatherService weatherService, IAlertService alertService, ILogger<WeatherController> logger)
        {
            _weatherService = weatherService;
            _alertService = alertService;
            _logger = logger;
        }

        [HttpGet("weather")]
        public async Task<IActionResult> GetWeather(string city = null, double? lat = null, double? lon = null)
        {
            WeatherResponse weather = null;
            string cityName = null;

            if (!string.IsNullOrEmpty(city))
            {
                weather = await _weatherService.GetWeatherAsync(city);
                cityName = weather?.Name;
            }
            else if (lat.HasValue && lon.HasValue)
            {
                cityName = await _weatherService.GetCityNameByCoordinatesAsync(lat.Value, lon.Value);
                weather = await _weatherService.GetWeatherByCoordinatesAsync(lat.Value, lon.Value);
            }
            else
            {
                return BadRequest(new { Message = "Please Send City or lat and lon." });
            }

            if (weather == null || string.IsNullOrEmpty(cityName))
            {
                _logger.LogWarning($"No weather data found for {city ?? $"{lat}, {lon}"}");
                return NotFound(new { Message = "لا توجد بيانات متاحة للطقس." });
            }

            var condition = weather.Weather?.FirstOrDefault()?.Main.ToLower();
            var temperature = weather.Main?.Temp;
            var humidity = weather.Main?.Humidity;

            string? alert = null;

            if (condition?.Contains("rain") == true || condition?.Contains("storm") == true)
            {
                alert = "⚠️ تحذير: هناك توقعات بهطول أمطار أو عواصف.";
            }
            else if (temperature > 40)
            {
                alert = "🔥 تحذير: درجة حرارة مرتفعة جداً.";
            }
            else if (temperature < 5)
            {
                alert = "❄️ تحذير: طقس بارد جداً، يُرجى أخذ الحيطة.";
            }
            else if (humidity > 80)
            {
                alert = "💦 تحذير: نسبة الرطوبة مرتفعة جداً.";
            }

            var existingAlert = await _alertService.GetAlertByCityAsync(weather.Name);

            if (!string.IsNullOrEmpty(alert))
            {
                // إذا لم يكن هناك تنبيه، أنشئ واحدًا
                if (existingAlert == null)
                {
                    _logger.LogInformation($"Alert triggered for {cityName}: {alert}");
                    await _alertService.StoreAlertAsync(new Alert
                    {
                        City = cityName,
                        Message = alert
                    });
                }
            }
            else
            {
                // إذا عادت الظروف إلى طبيعتها، احذف التنبيه الحالي
                if (existingAlert != null)
                {
                    _logger.LogInformation($"Conditions normalized. Removing alert for {cityName}.");
                    await _alertService.RemoveAlertByCityAsync(cityName);
                }
            }

            return Ok(new
            {
                City = cityName,
                Temperature = temperature,
                Humidity = humidity,
                Condition = weather.Weather?.FirstOrDefault()?.Description,
                Alert = alert
            });
        }

        //New
      //  [Authorize(Roles = "manager,Worker")]
        [HttpGet("alerts")]
        public async Task<IActionResult> GetAlerts()
        {
            var alerts = await _alertService.GetAllAlertsAsync(); // احصل على التحذيرات من الخدمة
            return Ok(alerts);
        }


    }
}
