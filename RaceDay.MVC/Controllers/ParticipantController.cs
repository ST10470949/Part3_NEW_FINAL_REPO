using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers
{
    public class ParticipantController : BaseController
    {
        private readonly IRaceDayApiService _api;

        public ParticipantController(IRaceDayApiService api)
        {
            _api = api;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enrol(int eventId, int categoryId)
        {
            var check = RequireRole("Participant");
            if (check != null) return check;

            var result = await _api.EnrolAsync(categoryId);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success
                    ? $"You're enrolled! Status: {result.Data?.Status}."
                    : result.ErrorMessage;

            return RedirectToAction("Details", "Home", new { id = eventId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelEnrolment(int enrolmentId)
        {
            var check = RequireRole("Participant");
            if (check != null) return check;

            var result = await _api.CancelEnrolmentAsync(enrolmentId);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "Enrolment cancelled." : result.ErrorMessage;

            return RedirectToAction("MyEnrolments");
        }

        public async Task<IActionResult> MyEnrolments()
        {
            var check = RequireRole("Participant");
            if (check != null) return check;

            var result = await _api.GetMyEnrolmentsAsync();
            if (!result.Success)
            {
                ViewBag.ErrorMessage = result.ErrorMessage;
                return View(new List<EnrolmentViewModel>());
            }

            var enrolments = result.Data!.OrderBy(e => e.EnrolmentDate).Select(e => new EnrolmentViewModel
            {
                EnrolmentID = e.EnrolmentID,
                EventID = e.EventID,
                EventName = e.EventName,
                CategoryID = e.CategoryID,
                CategoryName = e.CategoryName,
                EnrolmentDate = e.EnrolmentDate,
                Status = e.Status
            }).ToList();

            return View(enrolments);
        }

        public async Task<IActionResult> MyResults()
        {
            var check = RequireRole("Participant");
            if (check != null) return check;

            var result = await _api.GetMyResultsAsync();
            if (!result.Success)
            {
                ViewBag.ErrorMessage = result.ErrorMessage;
                return View(new List<ResultViewModel>());
            }

            var results = result.Data!.Select(r => new ResultViewModel
            {
                ResultID = r.ResultID,
                EnrolmentID = r.EnrolmentID,
                EventName = r.EventName,
                EventDate = r.EventDate,
                CategoryName = r.CategoryName,
                FinishTime = r.FinishTime,
                Position = r.Position,
                TotalFinishersInCategory = r.TotalFinishersInCategory
            }).ToList();

            return View(results);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var check = RequireLogin();
            if (check != null) return check;

            var result = await _api.GetMyProfileAsync();
            if (!result.Success || result.Data == null)
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Couldn't load your profile.";
                return RedirectToAction("Index", "Home");
            }

            var model = new ProfileViewModel
            {
                UserID = result.Data.UserID,
                FullName = result.Data.FullName,
                Email = result.Data.Email,
                Role = result.Data.Role,
                CreatedAt = result.Data.CreatedAt,
                ProfilePictureUrl = result.Data.ProfilePictureUrl
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            var check = RequireLogin();
            if (check != null) return check;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var updateResult = await _api.UpdateMyProfileAsync(model.FullName, model.Email);
            if (!updateResult.Success)
            {
                ModelState.AddModelError(string.Empty, updateResult.ErrorMessage ?? "Couldn't update your profile.");
                return View(model);
            }

            HttpContext.Session.SetString(SessionFullName, model.FullName);

            if (model.ProfilePictureFile != null)
            {
                var upload = await _api.UploadProfilePictureAsync(model.ProfilePictureFile);
                if (upload.Success && upload.Data != null)
                {
                    HttpContext.Session.SetString(SessionProfilePictureUrl, upload.Data);
                }
                else
                {
                    TempData["ErrorMessage"] = $"Profile updated, but the picture upload failed: {upload.ErrorMessage}";
                    return RedirectToAction("Profile");
                }
            }

            TempData["SuccessMessage"] = "Profile updated.";
            return RedirectToAction("Profile");
        }
    }
}
