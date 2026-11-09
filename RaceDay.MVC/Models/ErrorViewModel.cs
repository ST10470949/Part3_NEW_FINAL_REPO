namespace RaceDay.MVC.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        // A friendly message for expected failures (API down, 500 from API, etc.) -
        // set this instead of ever showing a raw exception to the user.
        public string? FriendlyMessage { get; set; }
    }
}
