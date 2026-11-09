using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RaceDay.MVC.Models
{
    // The Create/Edit Event form. EventID is null for a new event.
    public class EventFormViewModel
    {
        public int? EventID { get; set; }

        [Required(ErrorMessage = "Give the event a name.")]
        [Display(Name = "Event name")]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required(ErrorMessage = "Pick a date for the event.")]
        [DataType(DataType.Date)]
        [Display(Name = "Event date")]
        public DateTime EventDate { get; set; } = DateTime.Today.AddMonths(1);

        [Required(ErrorMessage = "Where is the event happening?")]
        public string Location { get; set; } = string.Empty;

        [Range(0.1, 5000, ErrorMessage = "Distance must be more than 0km.")]
        [Display(Name = "Distance (km)")]
        public decimal DistanceKm { get; set; } = 5;

        [Required(ErrorMessage = "Choose an event type.")]
        [Display(Name = "Event type")]
        public string EventType { get; set; } = "Run";

        [Display(Name = "Banner image")]
        public IFormFile? BannerFile { get; set; }

        public string? ExistingBannerImageUrl { get; set; }
    }
}
