using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Controllers;
using RaceDay.API.Data;
using RaceDay.API.DTOs.Auth;
using RaceDay.API.Services;
using RaceDay.Tests.TestHelpers;
using Xunit;

namespace RaceDay.Tests
{
    public class AuthControllerTests
    {
        // PasswordHashService has no dependencies of its own, so the real thing is
        // used here rather than a mock - we actually want to prove hashing works.
        private static AuthController CreateController(RaceDayContext context)
        {
            return new AuthController(context, new PasswordHashService())
            {
                ControllerContext = TestDbFactory.BuildControllerContext()
            };
        }

        [Fact]
        public async Task Register_WithValidData_ReturnsCreated()
        {
            var context = TestDbFactory.CreateContext();
            var controller = CreateController(context);

            var dto = new RegisterDto
            {
                FullName = "Lindiwe Dlamini",
                Email = "lindiwe@example.com",
                Password = "Password123",
                Role = "Participant"
            };

            var result = await controller.Register(dto);

            var created = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, created.StatusCode);
            Assert.Single(context.Users);
        }

        [Fact]
        public async Task Register_StoresAHashNotThePlainPassword()
        {
            var context = TestDbFactory.CreateContext();
            var controller = CreateController(context);

            await controller.Register(new RegisterDto
            {
                FullName = "Lindiwe Dlamini",
                Email = "lindiwe2@example.com",
                Password = "Password123",
                Role = "Participant"
            });

            var savedUser = Assert.Single(context.Users);
            Assert.NotEqual("Password123", savedUser.PasswordHash);
            Assert.False(string.IsNullOrWhiteSpace(savedUser.PasswordHash));
        }

        [Fact]
        public async Task Register_WithDuplicateEmail_ReturnsConflict()
        {
            var context = TestDbFactory.CreateContext();
            var controller = CreateController(context);

            var dto = new RegisterDto
            {
                FullName = "Sipho Nkosi",
                Email = "sipho@example.com",
                Password = "Password123",
                Role = "Participant"
            };

            await controller.Register(dto);

            // Second registration, same email
            var secondResult = await controller.Register(dto);

            Assert.IsType<ConflictObjectResult>(secondResult);
        }

        [Fact]
        public async Task Login_WithCorrectCredentials_ReturnsOkAndStartsSession()
        {
            var context = TestDbFactory.CreateContext();
            var controller = CreateController(context);

            await controller.Register(new RegisterDto
            {
                FullName = "Thandiwe Mokoena",
                Email = "thandiwe@example.com",
                Password = "MyStrongPass1",
                Role = "Organiser"
            });

            var loginResult = await controller.Login(new LoginDto
            {
                Email = "thandiwe@example.com",
                Password = "MyStrongPass1"
            });

            Assert.IsType<OkObjectResult>(loginResult);

            var storedUserId = controller.ControllerContext.HttpContext.Session.GetInt32("UserId");
            var storedRole = controller.ControllerContext.HttpContext.Session.GetString("Role");
            Assert.NotNull(storedUserId);
            Assert.Equal("Organiser", storedRole);
        }

        [Fact]
        public async Task Login_WithWrongPassword_ReturnsUnauthorized()
        {
            var context = TestDbFactory.CreateContext();
            var controller = CreateController(context);

            await controller.Register(new RegisterDto
            {
                FullName = "Johan van der Merwe",
                Email = "johan@example.com",
                Password = "CorrectPass1",
                Role = "Organiser"
            });

            var loginResult = await controller.Login(new LoginDto
            {
                Email = "johan@example.com",
                Password = "WrongPassword"
            });

            Assert.IsType<UnauthorizedObjectResult>(loginResult);
        }
    }
}
