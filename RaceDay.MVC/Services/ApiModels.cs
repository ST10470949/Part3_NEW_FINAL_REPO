namespace RaceDay.MVC.Services
{
    // These mirror the JSON shapes the Part 2 API returns. Kept separate from the
    // MVC ViewModels on purpose - this file is "what the API gives us", the
    // ViewModels are "what a Razor view needs", and those two things don't always
    // line up one-to-one.

    public class ApiLoginResult
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class ApiUserProfile
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }

    public class ApiEvent
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

    public class ApiEventDetail : ApiEvent
    {
        public List<ApiCategory> Categories { get; set; } = new();
    }

    public class ApiCategory
    {
        public int CategoryID { get; set; }
        public int EventID { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal EntryFee { get; set; }
        public int MaxParticipants { get; set; }
    }

    public class ApiEnrolment
    {
        public int EnrolmentID { get; set; }
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int EventID { get; set; }
        public string EventName { get; set; } = string.Empty;
        public int ParticipantID { get; set; }
        public string? ParticipantName { get; set; }
        public DateTime EnrolmentDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class ApiResult
    {
        public int ResultID { get; set; }
        public int EnrolmentID { get; set; }
        public string? ParticipantName { get; set; }
        public string? EventName { get; set; }
        public DateTime EventDate { get; set; }
        public string? CategoryName { get; set; }
        public TimeSpan? FinishTime { get; set; }
        public int? Position { get; set; }
        public int TotalFinishersInCategory { get; set; }
    }

    // Small helper the API sends back on most error responses: { "message": "..." }
    public class ApiErrorBody
    {
        public string? Message { get; set; }
    }
}
