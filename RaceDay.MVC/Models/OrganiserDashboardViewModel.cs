namespace RaceDay.MVC.Models
{
    public class OrganiserDashboardViewModel
    {
        public int TotalEvents { get; set; }
        public int TotalEnrolments { get; set; }
        public List<EventSummaryViewModel> Events { get; set; } = new();
    }

    public class EventSummaryViewModel
    {
        public int EventID { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string? BannerImageUrl { get; set; }
        public int EnrolmentCount { get; set; }
    }
}
