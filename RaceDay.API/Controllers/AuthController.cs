using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs.Auth;
using RaceDay.API.Models;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers
{
    /// <summary>
    /// Handles registering new accounts and logging in. Successful login starts
    /// a server-side session that every other controller reads from.
    /// </summary>
    [Route("api/auth")]
    public class AuthController : BaseApiController
    {
        private readonly RaceDayContext _context;
        private readonly IPasswordHashService _passwordHashService;

        public AuthController(RaceDayContext context, IPasswordHashService passwordHashService)
        {
            _context = context;
            _passwordHashService = passwordHashService;
        }

        /// <summary>Registers a new Organiser or Participant account.</summary>
        /// <param name="dto">Full name, email, password and chosen role.</param>
        /// <response code="201">Account created.</response>
        /// <response code="400">Validation failed or the role isn't recognised.</response>
        /// <response code="409">An account with that email already exists.</response>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!Enum.TryParse<UserRole>(dto.Role, true, out var parsedRole))
            {
                return BadRequest(new { message = "Role must be either 'Organiser' or 'Participant'." });
            }

            var emailTaken = await _context.Users
                .AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower());

            if (emailTaken)
            {
                return Conflict(new { message = "An account with that email already exists." });
            }

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Role = parsedRole,
                CreatedAt = DateTime.UtcNow
            };

            // Hash is generated from the user object + plain password - never save the raw password.
            user.PasswordHash = _passwordHashService.HashPassword(user, dto.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return StatusCode(StatusCodes.Status201Created, new
            {
                user.UserID,
                user.FullName,
                user.Email,
                Role = user.Role.ToString()
            });
        }

        /// <summary>Logs a user in and starts their session (stores UserId + Role).</summary>
        /// <param name="dto">Email and password.</param>
        /// <response code="200">Login successful, session started.</response>
        /// <response code="401">Email or password is incorrect.</response>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            var passwordOk = _passwordHashService.VerifyPassword(user, dto.Password);

            if (!passwordOk)
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            // This is what every other controller checks - no session, no access.
            HttpContext.Session.SetInt32(SessionUserId, user.UserID);
            HttpContext.Session.SetString(SessionUserRole, user.Role.ToString());
            HttpContext.Session.SetString(SessionFullName, user.FullName);

            return Ok(new
            {
                message = "Login successful.",
                user.UserID,
                user.FullName,
                user.Email,
                Role = user.Role.ToString()
            });
        }

        /// <summary>Ends the current session.</summary>
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { message = "Logged out." });
        }
    }
}
