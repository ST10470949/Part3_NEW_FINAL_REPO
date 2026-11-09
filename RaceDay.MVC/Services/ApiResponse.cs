namespace RaceDay.MVC.Services
{
    // Every call to the API comes back through one of these instead of throwing,
    // so controllers can just check Success and show a friendly message on failure
    // rather than needing a try/catch around every single API call.
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? ErrorMessage { get; set; }
        public int StatusCode { get; set; }

        public static ApiResponse<T> Ok(T data, int statusCode = 200) =>
            new() { Success = true, Data = data, StatusCode = statusCode };

        public static ApiResponse<T> Fail(string errorMessage, int statusCode = 0) =>
            new() { Success = false, ErrorMessage = errorMessage, StatusCode = statusCode };
    }
}
