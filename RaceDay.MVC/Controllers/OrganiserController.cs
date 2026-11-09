using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers
{
    // Every action here is Organiser-only - RequireRole at the top of each action
    // sends anyone else (including a logged-out visitor) somewhere sensible instead
    // of showing them an Organiser page.
    public class OrganiserController : BaseController
    {
        private readonly IRaceDayApiService _api;

        public OrganiserController(IRaceDayApiService api)
        {
            _api = api;
        }

        public async Task<IActionResult> Dashboard()
        {
            var check = RequireRole("Organiser");
            if (check != null) return check;

            ViewBag.OrganiserName = CurrentFullName;

            var eventsResult = await _api.GetMyEventsAsync();
            if (!eventsResult.Success)
            {
                ViewBag.ErrorMessage = eventsResult.ErrorMessage;
                return View(new OrganiserDashboardViewModel());
            }

            var summaries = new List<EventSummaryViewModel>();
            var totalEnrolments = 0;

            foreach (var ev in eventsResult.Data!)
            {
                var enrolmentsResult = await _api.GetEventEnrolmentsAsync(ev.EventID);
                var count = enrolmentsResult.Success ? enrolmentsResult.Data?.Count ?? 0 : 0;
                totalEnrolments += count;

                summaries.Add(new EventSummaryViewModel
                {
                    EventID = ev.EventID,
                    Name = ev.Name,
                    EventDate = ev.EventDate,
                    Location = ev.Location,
                    BannerImageUrl = ev.BannerImageUrl,
                    EnrolmentCount = count
                });
            }

            var viewModel = new OrganiserDashboardViewModel
            {
                TotalEvents = summaries.Count,
                TotalEnrolments = totalEnrolments,
                Events = summaries.OrderBy(e => e.EventDate).ToList()
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Events()
        {
            var check = RequireRole("Organiser");
            if (check != null) return check;

            var result = await _api.GetMyEventsAsync();
            if (!result.Success)
            {
                ViewBag.ErrorMessage = result.ErrorMessage;
                return View(new List<EventViewModel>());
            }

            var events = result.Data!.OrderBy(e => e.EventDate).Select(e => new EventViewModel
            {
                EventID = e.EventID,
                Name = e.Name,
                EventDate = e.EventDate,
                Location = e.Location,
                Description = e.Description,
                DistanceKm = e.DistanceKm,
                EventType = e.EventType,
                BannerImageUrl = e.BannerImageUrl,
                OrganiserID = e.OrganiserID,
                OrganiserName = e.OrganiserName
            }).ToList();

            return View(events);
        }

        [HttpGet]
        public IActionResult CreateEvent()
        {
            var check = RequireRole("Organiser");
            if (check != null) return check;

            return View(new EventFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(EventFormViewModel model)
        {
            var check = RequireRole("Organiser");
            if (check != null) return check;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _api.CreateEventAsync(model.Name, model.Description, model.EventDate, model.Location, model.DistanceKm, model.EventType);

            if (!result.Success || result.Data == null)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Couldn't create the event. Please try again.");
                return View(model);
            }

            var bannerUploadFailed = false;
            if (model.BannerFile != null)
            {
                var upload = await _api.UploadEventBannerAsync(result.Data.EventID, model.BannerFile);
                if (!upload.Success)
                {
                    bannerUploadFailed = true;
                    TempData["ErrorMessage"] = $"Event created, but the banner upload failed: {upload.ErrorMessage}";
                }
            }

            if (!bannerUploadFailed)
            {
                TempData["SuccessMessage"] = "Event created.";
            }
            return RedirectToAction("Events");
        }

        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {
            var check = RequireRole("Organiser");
            if (check != null) return check;

            var result = await _api.GetEventAsync(id);
            if (!result.Success || result.Data == null)
            {
                TempData["ErrorMessage"] = "That event couldn't be found.";
                return RedirectToAction("Events");
            }

            if (result.Data.OrganiserID != CurrentUserId!.Value)
            {
                TempData["ErrorMessage"] = "You can only edit your own events.";
                return RedirectToAction("Events");
            }

            var model = new EventFormViewModel
            {
                EventID = result.Data.EventID,
                Name = result.Data.Name,
                Description = result.Data.Description,
                EventDate = result.Data.EventDate,
                Location = result.Data.Location,
                DistanceKm = result.Data.DistanceKm,
                EventType = result.Data.EventType,
                ExistingBannerImageUrl = result.Data.BannerImageUrl
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEvent(int id, EventFormViewModel model)
        {
            var check = RequireRole("Organiser");
            if (check != null) return check;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _api.UpdateEventAsync(id, model.Name, model.Description, model.EventDate, model.Location, model.DistanceKm, model.EventType);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Couldn't update the event. Please try again.");
                return View(model);
            }

            var bannerUploadFailed = false;
            if (model.BannerFile != null)
            {
                var upload = await _api.UploadEventBannerAsync(id, model.BannerFile);
                if (!upload.Success)
                {
                    bannerUploadFailed = true;
                    TempData["ErrorMessage"] = $"Event updated, but the banner upload failed: {upload.ErrorMessage}";
                }
            }

            if (!bannerUploadFailed)
            {
                TempData["SuccessMessage"] = "Event updated.";
            }
            return RedirectToAction("Events");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var check = RequireRole("Organiser");
            if (check != null) return check;

            var result = await _api.DeleteEventAsync(id);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "Event deleted." : result.ErrorMessage;

            return RedirectToAction("Events");
        }

        [HttpGet]
        public async Task<IActionResult> Categories(int eventId)
        {
            var check = RequireRole("Organiser");
            if (check != null) return check;

            var eventResult = await _api.GetEventAsync(eventId);
            if (!eventResult.Success || eventResult.Data == null)
            {
                TempData["ErrorMessage"] = "That event couldn't be found.";
                return RedirectToAction("Events");
            }

            var model = new CreateCategoryViewModel
            {
                EventID = eventId,
                EventName = eventResult.Data.Name,
                ExistingCategories = eventResult.Data.Categories.Select(c => new CategoryViewModel
                {
                    CategoryID = c.CategoryID,
                    EventID = c.EventID,
                    Name = c.Name,
                    EntryFee = c.EntryFee,
                    MaxParticipants = c.MaxParticipants
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Categories(CreateCategoryViewModel model)
        {
            var check = RequireRole("Organiser");
            if (check != null) return check;

            if (!ModelState.IsValid)
            {
                var eventResult = await _api.GetEventAsync(model.EventID);
                model.EventName = eventResult.Data?.Name;
                model.ExistingCategories = eventResult.Data?.Categories.Select(c => new CategoryViewModel
                {
                    CategoryID = c.CategoryID,
                    EventID = c.EventID,
                    Name = c.Name,
                    EntryFee = c.EntryFee,
                    MaxParticipants = c.MaxParticipants
                }).ToList() ?? new List<CategoryViewModel>();
                return View(model);
            }

            var result = await _api.CreateCategoryAsync(model.EventID, model.Name, model.EntryFee, model.MaxParticipants);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "Category added." : result.ErrorMessage;

            return RedirectToAction("Categories", new { eventId = model.EventID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCategory(int eventId, int categoryId, string name, decimal entryFee, int maxParticipants)
        {
            var check = RequireRole("Organiser");
            if (check != null) return check;

            if (string.IsNullOrWhiteSpace(name) || entryFee < 0 || maxParticipants < 1)
            {
                TempData["ErrorMessage"] = "Enter a category name, a non-negative entry fee, and at least one participant.";
                return RedirectToAction("Categories", new { eventId });
            }

            var result = await _api.UpdateCategoryAsync(categoryId, name.Trim(), entryFee, maxParticipants);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "Category updated." : result.ErrorMessage;
            return RedirectToAction("Categories", new { eventId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int eventId, int categoryId)
        {
            var check = RequireRole("Organiser");
            if (check != null) return check;

            var result = await _api.DeleteCategoryAsync(categoryId);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "Category deleted." : result.ErrorMessage;
            return RedirectToAction("Categories", new { eventId });
        }

        public async Task<IActionResult> Enrolments(int eventId)
        {
            var check = RequireRole("Organiser");
            if (check != null) return check;

            var eventResult = await _api.GetEventAsync(eventId);
            var enrolmentsResult = await _api.GetEventEnrolmentsAsync(eventId);

            if (!enrolmentsResult.Success)
            {
                ViewBag.ErrorMessage = enrolmentsResult.ErrorMessage;
            }

            ViewBag.EventId = eventId;
            ViewBag.EventName = eventResult.Data?.Name ?? "Event";

            var enrolments = (enrolmentsResult.Data ?? new List<ApiEnrolment>()).Select(e => new EnrolmentViewModel
            {
                EnrolmentID = e.EnrolmentID,
                EventID = e.EventID,
                EventName = e.EventName,
                CategoryID = e.CategoryID,
                CategoryName = e.CategoryName,
                ParticipantName = e.ParticipantName,
                EnrolmentDate = e.EnrolmentDate,
                Status = e.Status
            }).ToList();

            return View(enrolments);
        }

        [HttpGet]
        public async Task<IActionResult> Results(int eventId)
        {
            var check = RequireRole("Organiser");
            if (check != null) return check;

            var eventResult = await _api.GetEventAsync(eventId);
            var enrolmentsResult = await _api.GetEventEnrolmentsAsync(eventId);
            var resultsResult = await _api.GetEventResultsAsync(eventId);

            ViewBag.EventId = eventId;
            ViewBag.EventName = eventResult.Data?.Name ?? "Event";

            var capturedEnrolmentIds = (resultsResult.Data ?? new List<ApiResult>())
                .Select(r => r.EnrolmentID)
                .ToHashSet();

            var rows = (enrolmentsResult.Data ?? new List<ApiEnrolment>()).Select(e => new CaptureResultViewModel
            {
                EnrolmentID = e.EnrolmentID,
                ParticipantName = e.ParticipantName,
                CategoryName = e.CategoryName,
                AlreadyCaptured = capturedEnrolmentIds.Contains(e.EnrolmentID)
            }).ToList();

            return View(rows);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CaptureResult(int eventId, int enrolmentId, TimeSpan? finishTime, int? position)
        {
            var check = RequireRole("Organiser");
            if (check != null) return check;

            var result = await _api.CaptureResultAsync(enrolmentId, finishTime, position);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "Result captured." : result.ErrorMessage;

            return RedirectToAction("Results", new { eventId });
        }
    }
}
