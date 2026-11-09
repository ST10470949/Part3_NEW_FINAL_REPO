namespace RaceDay.API.DTOs.Weather
{
    public class WeatherDto
    {
        public int EventID { get; set; }
        public string? WeatherForecast { get; set; }
        public string? RouteMapUrl { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
