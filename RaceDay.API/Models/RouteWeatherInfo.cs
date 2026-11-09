using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    // Race-day prep info an Organiser can attach to their Event.
    public class RouteWeatherInfo
    {
        [Key]
        public int RouteInfoID { get; set; }

        [Required]
        public int EventID { get; set; }

        public string? WeatherForecast { get; set; }

        public string? RouteMapURL { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(EventID))]
        public Event? Event { get; set; }
    }
}
