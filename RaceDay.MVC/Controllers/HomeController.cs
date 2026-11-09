using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers
{
    // The public-facing side of the site - anyone can browse events here, logged
    // in or not. Enrolling is the only bit that needs a Participant session.
    public class HomeController : BaseController
    {
        private readonly IRaceDayApiService _api;

        public HomeController(IRaceDayApiService api)
        {
            _api = api;
        }

        public async Task<IActionResult> Index(string? location, string? eventType, DateTime? date)
        {
            var result = await _api.GetEventsAsync(location, eventType, date);

            if (!result.Success)
            {
                ViewBag.ErrorMessage = result.ErrorMessage;
                return View(new List<EventViewModel>());
            }

            var events = result.Data!
                .Where(e => e.EventDate >= DateTime.Today)
                .OrderBy(e => e.EventDate)
                .Select(MapToViewModel)
                .ToList();

            ViewBag.Location = location;
            ViewBag.EventType = eventType;
            ViewBag.Date = date?.ToString("yyyy-MM-dd");

            return View(events);
        }

        public async Task<IActionResult> Details(int id)
        {
            var result = await _api.GetEventAsync(id);

            if (!result.Success || result.Data == null)
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "That event couldn't be found.";
                return RedirectToAction("Index");
            }

            var eventDetail = result.Data;

            var viewModel = new EventDetailsViewModel
            {
                Event = MapToViewModel(eventDetail),
                Categories = eventDetail.Categories.Select(c => new CategoryViewModel
                {
                    CategoryID = c.CategoryID,
                    EventID = c.EventID,
                    Name = c.Name,
                    EntryFee = c.EntryFee,
                    MaxParticipants = c.MaxParticipants
                }).ToList(),
                IsLoggedIn = IsLoggedIn,
                IsParticipant = string.Equals(CurrentRole, "Participant", StringComparison.OrdinalIgnoreCase)
            };

            if (viewModel.IsParticipant)
            {
                var myEnrolments = await _api.GetMyEnrolmentsAsync();
                if (myEnrolments.Success && myEnrolments.Data != null)
                {
                    viewModel.AlreadyEnrolledCategoryIds = myEnrolments.Data
                        .Where(e => e.EventID == id)
                        .Select(e => e.CategoryID)
                        .ToList();
                }
            }

            return View(viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
        }

        private static EventViewModel MapToViewModel(ApiEvent e) => new()
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
        };
    }
}
