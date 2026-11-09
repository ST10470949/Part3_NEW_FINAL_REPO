namespace RaceDay.MVC.Models
{
    // Powers the public event details page - the event itself, its categories,
    // and whether the current visitor (if a Participant) can still enrol.
    public class EventDetailsViewModel
    {
        public EventViewModel Event { get; set; } = new();
        public List<CategoryViewModel> Categories { get; set; } = new();
        public bool IsLoggedIn { get; set; }
        public bool IsParticipant { get; set; }
        public List<int> AlreadyEnrolledCategoryIds { get; set; } = new();
    }
}
