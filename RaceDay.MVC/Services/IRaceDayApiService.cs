using Microsoft.AspNetCore.Http;

namespace RaceDay.MVC.Services
{
    // Every controller talks to the API through this one interface instead of
    // building HttpClient calls itself. Keeps all the "how do we call the API"
    // detail (cookies, JSON, multipart uploads) in one place.
    public interface IRaceDayApiService
    {
        Task<ApiResponse<ApiLoginResult>> LoginAsync(string email, string password);
        Task<ApiResponse<bool>> RegisterAsync(string fullName, string email, string password, string role);
        Task LogoutAsync();

        Task<ApiResponse<List<ApiEvent>>> GetEventsAsync(string? location = null, string? eventType = null, DateTime? date = null);
        Task<ApiResponse<List<ApiEvent>>> GetMyEventsAsync();
        Task<ApiResponse<ApiEventDetail>> GetEventAsync(int id);
        Task<ApiResponse<ApiEvent>> CreateEventAsync(string name, string? description, DateTime eventDate, string location, decimal distanceKm, string eventType);
        Task<ApiResponse<ApiEvent>> UpdateEventAsync(int id, string name, string? description, DateTime eventDate, string location, decimal distanceKm, string eventType);
        Task<ApiResponse<bool>> DeleteEventAsync(int id);
        Task<ApiResponse<string>> UploadEventBannerAsync(int eventId, IFormFile file);

        Task<ApiResponse<List<ApiCategory>>> GetEventCategoriesAsync(int eventId);
        Task<ApiResponse<ApiCategory>> CreateCategoryAsync(int eventId, string name, decimal entryFee, int maxParticipants);
        Task<ApiResponse<ApiCategory>> UpdateCategoryAsync(int categoryId, string name, decimal entryFee, int maxParticipants);
        Task<ApiResponse<bool>> DeleteCategoryAsync(int categoryId);

        Task<ApiResponse<List<ApiEnrolment>>> GetEventEnrolmentsAsync(int eventId);
        Task<ApiResponse<List<ApiResult>>> GetEventResultsAsync(int eventId);

        Task<ApiResponse<ApiEnrolment>> EnrolAsync(int categoryId);
        Task<ApiResponse<List<ApiEnrolment>>> GetMyEnrolmentsAsync();
        Task<ApiResponse<bool>> CancelEnrolmentAsync(int enrolmentId);

        Task<ApiResponse<ApiResult>> CaptureResultAsync(int enrolmentId, TimeSpan? finishTime, int? position);
        Task<ApiResponse<List<ApiResult>>> GetMyResultsAsync();

        Task<ApiResponse<ApiUserProfile>> GetMyProfileAsync();
        Task<ApiResponse<ApiUserProfile>> UpdateMyProfileAsync(string fullName, string email);
        Task<ApiResponse<string>> UploadProfilePictureAsync(IFormFile file);
    }
}
