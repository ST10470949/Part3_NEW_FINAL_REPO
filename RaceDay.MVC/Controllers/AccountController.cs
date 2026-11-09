using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers
{
    public class AccountController : BaseController
    {
        private readonly IRaceDayApiService _api;

        public AccountController(IRaceDayApiService api)
        {
            _api = api;
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (IsLoggedIn) return RedirectToAction("Index", "Home");
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _api.RegisterAsync(model.FullName, model.Email, model.Password, model.Role);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Registration failed. Please check your details.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Account created. You can log in now.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (IsLoggedIn) return RedirectToAction("Index", "Home");
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _api.LoginAsync(model.Email, model.Password);

            if (!result.Success || result.Data == null)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Invalid email or password.");
                return View(model);
            }

            HttpContext.Session.SetInt32(SessionUserId, result.Data.UserID);
            HttpContext.Session.SetString(SessionRole, result.Data.Role);
            HttpContext.Session.SetString(SessionFullName, result.Data.FullName);

            // Grab the profile picture too so the nav bar avatar has something to show
            // straight after login rather than waiting for the next page load.
            var profile = await _api.GetMyProfileAsync();
            if (profile.Success && profile.Data?.ProfilePictureUrl != null)
            {
                HttpContext.Session.SetString(SessionProfilePictureUrl, profile.Data.ProfilePictureUrl);
            }

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return result.Data.Role == "Organiser"
                ? RedirectToAction("Dashboard", "Organiser")
                : RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _api.LogoutAsync();
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
