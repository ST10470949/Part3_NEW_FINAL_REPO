using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Controllers;
using RaceDay.API.DTOs.Events;
using RaceDay.API.Models;
using RaceDay.Tests.TestHelpers;
using Xunit;

namespace RaceDay.Tests
{
    public class EventsControllerTests
    {
        private static User SeedOrganiser(RaceDay.API.Data.RaceDayContext context)
        {
            var organiser = new User
            {
                FullName = "Thandiwe Mokoena",
                Email = "thandiwe@raceday.co.za",
                PasswordHash = "irrelevant-for-this-test",
                Role = UserRole.Organiser
            };
            context.Users.Add(organiser);
            context.SaveChanges();
            return organiser;
        }

        [Fact]
        public async Task Organiser_CanCreateEvent_ReturnsCreated()
        {
            var context = TestDbFactory.CreateContext();
            var organiser = SeedOrganiser(context);

            var controller = new EventsController(context, new FakeBlobStorageService())
            {
                ControllerContext = TestDbFactory.BuildControllerContext(organiser.UserID, "Organiser")
            };

            var result = await controller.CreateEvent(new CreateEventDto
            {
                Name = "Cape Town Cycle Tour",
                EventDate = DateTime.Today.AddMonths(2),
                Location = "Cape Town",
                Description = "109km cycling event."
            });

            var created = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, created.StatusCode);
            Assert.Single(context.Events);
        }

        [Fact]
        public async Task Participant_CannotCreateEvent_ReturnsForbidden()
        {
            var context = TestDbFactory.CreateContext();

            var controller = new EventsController(context, new FakeBlobStorageService())
            {
                ControllerContext = TestDbFactory.BuildControllerContext(userId: 5, role: "Participant")
            };

            var result = await controller.CreateEvent(new CreateEventDto
            {
                Name = "Should Not Be Created",
                EventDate = DateTime.Today.AddMonths(1),
                Location = "Durban"
            });

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, objectResult.StatusCode);
            Assert.Empty(context.Events);
        }

        [Fact]
        public async Task AnonymousUser_CannotCreateEvent_ReturnsUnauthorized()
        {
            var context = TestDbFactory.CreateContext();

            var controller = new EventsController(context, new FakeBlobStorageService())
            {
                ControllerContext = TestDbFactory.BuildControllerContext()
            };

            var result = await controller.CreateEvent(new CreateEventDto
            {
                Name = "Should Not Be Created",
                EventDate = DateTime.Today.AddMonths(1),
                Location = "Durban"
            });

            Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Empty(context.Events);
        }

        [Fact]
        public async Task GetEvents_IsPublic_ReturnsAllEventsWithoutLogin()
        {
            var context = TestDbFactory.CreateContext();
            var organiser = SeedOrganiser(context);

            context.Events.Add(new Event
            {
                OrganiserID = organiser.UserID,
                Name = "Durban Beachfront Park Run",
                EventDate = DateTime.Today.AddDays(10),
                Location = "Durban"
            });
            context.SaveChanges();

            var controller = new EventsController(context, new FakeBlobStorageService())
            {
                ControllerContext = TestDbFactory.BuildControllerContext() // nobody logged in
            };

            var result = await controller.GetEvents(location: null, date: null);

            var ok = Assert.IsType<OkObjectResult>(result);
            var events = Assert.IsAssignableFrom<System.Collections.IEnumerable>(ok.Value);
            Assert.NotEmpty(events.Cast<object>());
        }
    }
}
