using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace RaceDay.MVC.Services
{
    // Talks to the Part 2 API on behalf of whoever is browsing the MVC site.
    //
    // The tricky bit: the API uses a server-side session cookie for auth, not a
    // JWT you can just stick in an Authorization header. Because this HttpClient
    // is a typed client shared across every visitor (via IHttpClientFactory), we
    // can't let it keep its own cookie jar - that would leak one person's session
    // into another person's requests. So instead: cookies are switched off on the
    // handler, and we manually read/attach the API's session cookie from *this*
    // visitor's own MVC session on every call. That's what GetStoredApiCookie and
    // StoreApiCookie are doing below.
    public class RaceDayApiService : IRaceDayApiService
    {
        private const string ApiCookieSessionKey = "ApiSessionCookie";

        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RaceDayApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        // ---------- Auth ----------

        public async Task<ApiResponse<ApiLoginResult>> LoginAsync(string email, string password)
        {
            var response = await SendAsync(HttpMethod.Post, "api/auth/login", JsonContent.Create(new { email, password }));

            if (response.IsSuccessStatusCode)
            {
                CaptureApiCookie(response);
                var data = await response.Content.ReadFromJsonAsync<ApiLoginResult>();
                return data != null
                    ? ApiResponse<ApiLoginResult>.Ok(data, (int)response.StatusCode)
                    : ApiResponse<ApiLoginResult>.Fail("The server didn't return a valid login response.");
            }

            return ApiResponse<ApiLoginResult>.Fail(await ExtractErrorMessageAsync(response), (int)response.StatusCode);
        }

        public async Task<ApiResponse<bool>> RegisterAsync(string fullName, string email, string password, string role)
        {
            var response = await SendAsync(HttpMethod.Post, "api/auth/register",
                JsonContent.Create(new { fullName, email, password, role }));

            return response.IsSuccessStatusCode
                ? ApiResponse<bool>.Ok(true, (int)response.StatusCode)
                : ApiResponse<bool>.Fail(await ExtractErrorMessageAsync(response), (int)response.StatusCode);
        }

        public async Task LogoutAsync()
        {
            // Best effort - the API session will also just time out on its own,
            // but telling it explicitly is tidier.
            try
            {
                await SendAsync(HttpMethod.Post, "api/auth/logout");
            }
            catch (HttpRequestException)
            {
                // Not worth failing the user's logout over - clearing the local
                // session below is what actually matters for the MVC side.
            }

            _httpContextAccessor.HttpContext?.Session.Remove(ApiCookieSessionKey);
        }

        // ---------- Events ----------

        public async Task<ApiResponse<List<ApiEvent>>> GetEventsAsync(string? location = null, string? eventType = null, DateTime? date = null)
        {
            var query = new List<string>();
            if (!string.IsNullOrWhiteSpace(location)) query.Add($"location={Uri.EscapeDataString(location)}");
            if (!string.IsNullOrWhiteSpace(eventType)) query.Add($"eventType={Uri.EscapeDataString(eventType)}");
            if (date.HasValue) query.Add($"date={date.Value:yyyy-MM-dd}");

            var path = "api/events" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
            var response = await SendAsync(HttpMethod.Get, path);

            return await ParseListAsync<ApiEvent>(response);
        }

        public async Task<ApiResponse<List<ApiEvent>>> GetMyEventsAsync()
        {
            // Ownership filtering is performed by the Part 2 API, not by the MVC app.
            var response = await SendAsync(HttpMethod.Get, "api/events/mine");
            return await ParseListAsync<ApiEvent>(response);
        }

        public async Task<ApiResponse<ApiEventDetail>> GetEventAsync(int id)
        {
            var response = await SendAsync(HttpMethod.Get, $"api/events/{id}");
            return await ParseAsync<ApiEventDetail>(response);
        }

        public async Task<ApiResponse<ApiEvent>> CreateEventAsync(string name, string? description, DateTime eventDate, string location, decimal distanceKm, string eventType)
        {
            var response = await SendAsync(HttpMethod.Post, "api/events",
                JsonContent.Create(new { name, description, eventDate, location, distanceKm, eventType }));
            return await ParseAsync<ApiEvent>(response);
        }

        public async Task<ApiResponse<ApiEvent>> UpdateEventAsync(int id, string name, string? description, DateTime eventDate, string location, decimal distanceKm, string eventType)
        {
            var response = await SendAsync(HttpMethod.Put, $"api/events/{id}",
                JsonContent.Create(new { name, description, eventDate, location, distanceKm, eventType }));
            return await ParseAsync<ApiEvent>(response);
        }

        public async Task<ApiResponse<bool>> DeleteEventAsync(int id)
        {
            var response = await SendAsync(HttpMethod.Delete, $"api/events/{id}");
            return response.IsSuccessStatusCode
                ? ApiResponse<bool>.Ok(true, (int)response.StatusCode)
                : ApiResponse<bool>.Fail(await ExtractErrorMessageAsync(response), (int)response.StatusCode);
        }

        public async Task<ApiResponse<string>> UploadEventBannerAsync(int eventId, IFormFile file)
        {
            return await UploadFileAsync($"api/events/{eventId}/banner", file, "bannerImageUrl");
        }

        // ---------- Categories ----------

        public async Task<ApiResponse<List<ApiCategory>>> GetEventCategoriesAsync(int eventId)
        {
            var response = await SendAsync(HttpMethod.Get, $"api/events/{eventId}/categories");
            return await ParseListAsync<ApiCategory>(response);
        }

        public async Task<ApiResponse<ApiCategory>> CreateCategoryAsync(int eventId, string name, decimal entryFee, int maxParticipants)
        {
            var response = await SendAsync(HttpMethod.Post, $"api/events/{eventId}/categories",
                JsonContent.Create(new { name, entryFee, maxParticipants }));
            return await ParseAsync<ApiCategory>(response);
        }

        public async Task<ApiResponse<ApiCategory>> UpdateCategoryAsync(int categoryId, string name, decimal entryFee, int maxParticipants)
        {
            var response = await SendAsync(HttpMethod.Put, $"api/categories/{categoryId}",
                JsonContent.Create(new { name, entryFee, maxParticipants }));
            return await ParseAsync<ApiCategory>(response);
        }

        public async Task<ApiResponse<bool>> DeleteCategoryAsync(int categoryId)
        {
            var response = await SendAsync(HttpMethod.Delete, $"api/categories/{categoryId}");
            return response.IsSuccessStatusCode
                ? ApiResponse<bool>.Ok(true, (int)response.StatusCode)
                : ApiResponse<bool>.Fail(await ExtractErrorMessageAsync(response), (int)response.StatusCode);
        }

        // ---------- Organiser views of enrolments/results ----------

        public async Task<ApiResponse<List<ApiEnrolment>>> GetEventEnrolmentsAsync(int eventId)
        {
            var response = await SendAsync(HttpMethod.Get, $"api/events/{eventId}/enrolments");
            return await ParseListAsync<ApiEnrolment>(response);
        }

        public async Task<ApiResponse<List<ApiResult>>> GetEventResultsAsync(int eventId)
        {
            var response = await SendAsync(HttpMethod.Get, $"api/events/{eventId}/results");
            return await ParseListAsync<ApiResult>(response);
        }

        // ---------- Enrolments (Participant) ----------

        public async Task<ApiResponse<ApiEnrolment>> EnrolAsync(int categoryId)
        {
            var response = await SendAsync(HttpMethod.Post, "api/enrolments", JsonContent.Create(new { categoryId }));
            return await ParseAsync<ApiEnrolment>(response);
        }

        public async Task<ApiResponse<List<ApiEnrolment>>> GetMyEnrolmentsAsync()
        {
            var response = await SendAsync(HttpMethod.Get, "api/enrolments/me");
            return await ParseListAsync<ApiEnrolment>(response);
        }

        public async Task<ApiResponse<bool>> CancelEnrolmentAsync(int enrolmentId)
        {
            var response = await SendAsync(HttpMethod.Delete, $"api/enrolments/{enrolmentId}");
            return response.IsSuccessStatusCode
                ? ApiResponse<bool>.Ok(true, (int)response.StatusCode)
                : ApiResponse<bool>.Fail(await ExtractErrorMessageAsync(response), (int)response.StatusCode);
        }

        // ---------- Results ----------

        public async Task<ApiResponse<ApiResult>> CaptureResultAsync(int enrolmentId, TimeSpan? finishTime, int? position)
        {
            var response = await SendAsync(HttpMethod.Post, "api/results",
                JsonContent.Create(new { enrolmentId, finishTime, position }));
            return await ParseAsync<ApiResult>(response);
        }

        public async Task<ApiResponse<List<ApiResult>>> GetMyResultsAsync()
        {
            var response = await SendAsync(HttpMethod.Get, "api/results/me");
            return await ParseListAsync<ApiResult>(response);
        }

        // ---------- Profile ----------

        public async Task<ApiResponse<ApiUserProfile>> GetMyProfileAsync()
        {
            var response = await SendAsync(HttpMethod.Get, "api/users/me");
            return await ParseAsync<ApiUserProfile>(response);
        }

        public async Task<ApiResponse<ApiUserProfile>> UpdateMyProfileAsync(string fullName, string email)
        {
            var response = await SendAsync(HttpMethod.Put, "api/users/me", JsonContent.Create(new { fullName, email }));
            return await ParseAsync<ApiUserProfile>(response);
        }

        public async Task<ApiResponse<string>> UploadProfilePictureAsync(IFormFile file)
        {
            return await UploadFileAsync("api/users/me/profile-picture", file, "profilePictureUrl");
        }

        // ---------- Shared plumbing ----------

        private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, HttpContent? content = null)
        {
            var request = new HttpRequestMessage(method, path);
            if (content != null)
            {
                request.Content = content;
            }

            var apiCookie = GetStoredApiCookie();
            if (!string.IsNullOrEmpty(apiCookie))
            {
                request.Headers.Add("Cookie", apiCookie);
            }

            try
            {
                return await _httpClient.SendAsync(request);
            }
            catch (HttpRequestException)
            {
                // Keep an unavailable API from crashing the MVC request with a raw
                // developer exception page. The UI can show a normal service message.
                return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                {
                    Content = JsonContent.Create(new ApiErrorBody
                    {
                        Message = "RaceDay API is unavailable. Start RaceDay.API and confirm its configured URL and port, then try again."
                    })
                };
            }
        }

        private async Task<ApiResponse<string>> UploadFileAsync(string path, IFormFile file, string urlFieldName)
        {
            using var content = new MultipartFormDataContent();
            await using var stream = file.OpenReadStream();
            using var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(streamContent, "file", file.FileName);

            var response = await SendAsync(HttpMethod.Post, path, content);

            if (!response.IsSuccessStatusCode)
            {
                return ApiResponse<string>.Fail(await ExtractErrorMessageAsync(response), (int)response.StatusCode);
            }

            var body = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            if (body != null && body.TryGetValue(urlFieldName, out var url))
            {
                return ApiResponse<string>.Ok(url, (int)response.StatusCode);
            }

            return ApiResponse<string>.Fail("The upload went through but the server didn't return an image URL.");
        }

        private static async Task<ApiResponse<T>> ParseAsync<T>(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<T>();
                return data != null
                    ? ApiResponse<T>.Ok(data, (int)response.StatusCode)
                    : ApiResponse<T>.Fail("The server didn't return the expected data.", (int)response.StatusCode);
            }

            return ApiResponse<T>.Fail(await ExtractErrorMessageAsync(response), (int)response.StatusCode);
        }

        private static async Task<ApiResponse<List<T>>> ParseListAsync<T>(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<List<T>>();
                return ApiResponse<List<T>>.Ok(data ?? new List<T>(), (int)response.StatusCode);
            }

            return ApiResponse<List<T>>.Fail(await ExtractErrorMessageAsync(response), (int)response.StatusCode);
        }

        private static async Task<string> ExtractErrorMessageAsync(HttpResponseMessage response)
        {
            try
            {
                var error = await response.Content.ReadFromJsonAsync<ApiErrorBody>();
                if (!string.IsNullOrWhiteSpace(error?.Message))
                {
                    return error.Message;
                }
            }
            catch
            {
                // Body wasn't JSON in the shape we expected - fall through to a generic message below
                // rather than let a parsing error bubble up as an unhandled exception on the page.
            }

            return response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => "You need to log in first.",
                HttpStatusCode.Forbidden => "You're not allowed to do that.",
                HttpStatusCode.NotFound => "That couldn't be found.",
                HttpStatusCode.Conflict => "That conflicts with something that already exists.",
                HttpStatusCode.BadRequest => "Please check the information you entered and try again.",
                _ => "The service is currently unavailable. Please try again later."
            };
        }

        private string? GetStoredApiCookie()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString(ApiCookieSessionKey);
        }

        private void CaptureApiCookie(HttpResponseMessage response)
        {
            if (!response.Headers.TryGetValues("Set-Cookie", out var cookies))
            {
                return;
            }

            // We only care about the session cookie the API's own session
            // middleware issues - not any other cookie a browser might set.
            var sessionCookie = cookies.FirstOrDefault(c => c.Contains(".AspNetCore.Session", StringComparison.OrdinalIgnoreCase));
            if (sessionCookie == null)
            {
                return;
            }

            var nameAndValue = sessionCookie.Split(';')[0];
            _httpContextAccessor.HttpContext?.Session.SetString(ApiCookieSessionKey, nameAndValue);
        }
    }
}
