namespace RaceDay.MVC.Models
{
    // Used for showing events - the public list, the Organiser's own events list,
    // and as the base display info on the event details page.
    public class EventViewModel
    {
        public int EventID { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal DistanceKm { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string? BannerImageUrl { get; set; }
        public int OrganiserID { get; set; }
        public string? OrganiserName { get; set; }
    }
}
