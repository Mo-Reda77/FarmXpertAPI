using System.ComponentModel.DataAnnotations;

namespace FarmXpert.Models
{
    public class WeatherResponse
    {
        public Coord Coord { get; set; }
        public List<Weather> Weather { get; set; }
        public Main Main { get; set; }
        public Wind Wind { get; set; }
        public Clouds Clouds { get; set; }
        public Sys Sys { get; set; }
        public int Visibility { get; set; }
        public string Name { get; set; }
    }

    public class Coord { public double Lon { get; set; } public double Lat { get; set; } }
    public class Weather { public string Main { get; set; } public string Description { get; set; } }
    public class Main { public double Temp { get; set; } public int Humidity { get; set; } }
    public class Wind { public double Speed { get; set; } }
    public class Clouds { public int All { get; set; } }
    public class Sys
    {
     
        public string Country { get; set; }
    }
}
