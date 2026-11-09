using Microsoft.AspNetCore.Mvc;

namespace RaceDay.API.Controllers
{
    // Every controller that needs to know "who is logged in" and "what role are they"
    // inherits from this instead of ControllerBase directly. Keeps the session-reading
    // logic in one place instead of copy-pasted into every action.
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        protected const string SessionUserId = "UserId";
        protected const string SessionUserRole = "Role";
        protected const string SessionFullName = "FullName";

        protected int? CurrentUserId => HttpContext.Session.GetInt32(SessionUserId);

        protected string? CurrentUserRole => HttpContext.Session.GetString(SessionUserRole);

        protected bool IsLoggedIn => CurrentUserId.HasValue;

        // Returns an IActionResult if the check fails, or null if it's fine to continue.
        // Usage: var check = RequireLogin(); if (check != null) return check;
        protected IActionResult? RequireLogin()
        {
            if (!IsLoggedIn)
            {
                return Unauthorized(new { message = "You need to log in first." });
            }

            return null;
        }

        protected IActionResult? RequireRole(string role)
        {
            var loginCheck = RequireLogin();
            if (loginCheck != null)
            {
                return loginCheck;
            }

            if (!string.Equals(CurrentUserRole, role, StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = $"Only {role}s are allowed to do this." });
            }

            return null;
        }
    }
}
