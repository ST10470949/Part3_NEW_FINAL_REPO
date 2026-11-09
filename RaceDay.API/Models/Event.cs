using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    // Added for Part 3 - the Organiser event form needs a distance and an event type,
    // and the event detail page needs a banner image.
    public enum EventType
    {
        Run,
        Walk,
        Cycle
    }

    public class Event
    {
        [Key]
        public int EventID { get; set; }

        [Required]
        public int OrganiserID { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        [MaxLength(150)]
        public string Location { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Column(TypeName = "decimal(6,2)")]
        public decimal DistanceKm { get; set; }

        [Required]
        public EventType EventType { get; set; } = EventType.Run;

        // Set once the Organiser uploads a banner through /api/events/{id}/banner.
        [MaxLength(500)]
        public string? BannerImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(OrganiserID))]
        public User? Organiser { get; set; }

        public ICollection<Category> Categories { get; set; } = new List<Category>();

        public RouteWeatherInfo? WeatherInfo { get; set; }
    }
}
