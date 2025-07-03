using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FarmXpert.Models;
using FarmXpert.Services;

namespace FarmXpert.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
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
            // إذا تم إرسال اسم المدينة فقط
            if (!string.IsNullOrEmpty(city))
            {
                var weather = await _weatherService.GetWeatherAsync(city);
                if (weather == null)
                {
                    _logger.LogWarning($"No weather data found for city: {city}");
                    return NotFound(new { Message = "لا توجد بيانات متاحة للطقس." });
                }

                string? alert = null;
                var condition = weather.Weather?.FirstOrDefault()?.Main.ToLower();
                var temperature = weather.Main?.Temp;
                var humidity = weather.Main?.Humidity;

                _logger.LogInformation($"Weather for {city} - Temp: {temperature}, Humidity: {humidity}, Condition: {condition}");

                // شروط التحذير
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

                // تخزين التحذير في قاعدة البيانات + إشعار فوري
                if (!string.IsNullOrEmpty(alert))
                {
                    _logger.LogInformation($"Alert triggered for {city}: {alert}");
                    await _alertService.StoreAlertAsync(new Alert
                    {
                        City = weather.Name,
                        Message = alert
                    });
                }

                return Ok(new
                {
                    City = weather.Name,
                    Temperature = temperature,
                    Humidity = humidity,
                    Condition = weather.Weather?.FirstOrDefault()?.Description,
                    Alert = alert
                });
            }

            // إذا تم إرسال الإحداثيات فقط (خط العرض والطول)
            if (lat.HasValue && lon.HasValue)
            {
                var cityName = await _weatherService.GetCityNameByCoordinatesAsync(lat.Value, lon.Value);
                if (string.IsNullOrEmpty(cityName))
                {
                    _logger.LogWarning($"No weather data found for coordinates: {lat}, {lon}");
                    return NotFound(new { Message = "لا توجد بيانات متاحة للطقس." });
                }

                var weather = await _weatherService.GetWeatherByCoordinatesAsync(lat.Value, lon.Value);
                if (weather == null)
                {
                    _logger.LogWarning($"No weather data found for coordinates: {lat}, {lon}");
                    return NotFound(new { Message = "لا توجد بيانات متاحة للطقس." });
                }

                string? alert = null;
                var condition = weather.Weather?.FirstOrDefault()?.Main.ToLower();
                var temperature = weather.Main?.Temp;
                var humidity = weather.Main?.Humidity;

                _logger.LogInformation($"Weather for {cityName} ({lat}, {lon}) - Temp: {temperature}, Humidity: {humidity}, Condition: {condition}");

                // شروط التحذير
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

                // تخزين التحذير في قاعدة البيانات + إشعار فوري
                if (!string.IsNullOrEmpty(alert))
                {
                    _logger.LogInformation($"Alert triggered for coordinates {lat}, {lon}: {alert}");
                    await _alertService.StoreAlertAsync(new Alert
                    {
                        City = weather.Name,
                        Message = alert
                    });
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

            // إذا لم يتم إرسال أي من البيانات (لا اسم مدينة ولا إحداثيات)
            return BadRequest(new { Message = "Please Send City or lat and lon." });
        }

    }
}
