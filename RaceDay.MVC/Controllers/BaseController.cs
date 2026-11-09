using Microsoft.AspNetCore.Mvc;

namespace RaceDay.MVC.Controllers
{
    // Same idea as the BaseApiController on the API side, just adapted for a web
    // app: instead of returning 401/403 JSON, a failed check sends the browser to
    // Login (or Home, if they're logged in as the wrong role).
    public abstract class BaseController : Controller
    {
        protected const string SessionUserId = "UserId";
        protected const string SessionRole = "Role";
        protected const string SessionFullName = "FullName";
        protected const string SessionProfilePictureUrl = "ProfilePictureUrl";

        protected int? CurrentUserId => HttpContext.Session.GetInt32(SessionUserId);
        protected string? CurrentRole => HttpContext.Session.GetString(SessionRole);
        protected string? CurrentFullName => HttpContext.Session.GetString(SessionFullName);
        protected bool IsLoggedIn => CurrentUserId.HasValue;

        protected IActionResult? RequireLogin()
        {
            if (!IsLoggedIn)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Request.Path + Request.QueryString });
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

            if (!string.Equals(CurrentRole, role, StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = $"That page is only available to {role}s.";
                return RedirectToAction("Index", "Home");
            }

            return null;
        }
    }
}
