using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;

namespace RaceDay.Tests.TestHelpers
{
    // Shared setup so every test file doesn't repeat the same boilerplate.
    public static class TestDbFactory
    {
        // A fresh in-memory database per test - InMemory provider means we don't need
        // an actual SQL Server instance running for CI/CD to run these tests.
        public static RaceDayContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new RaceDayContext(options);
        }

        // Builds a ControllerContext with a fake session already "logged in" as the
        // given user, or with an empty session if userId is null (anonymous).
        public static ControllerContext BuildControllerContext(int? userId = null, string? role = null)
        {
            var httpContext = new DefaultHttpContext
            {
                Session = new FakeSession()
            };

            if (userId.HasValue)
            {
                httpContext.Session.SetInt32("UserId", userId.Value);
            }

            if (role != null)
            {
                httpContext.Session.SetString("Role", role);
            }

            return new ControllerContext
            {
                HttpContext = httpContext
            };
        }
    }
}
