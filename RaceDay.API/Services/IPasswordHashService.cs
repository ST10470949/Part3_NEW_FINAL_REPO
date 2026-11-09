using RaceDay.API.Models;

namespace RaceDay.API.Services
{
    // Wraps ASP.NET Core's PasswordHasher so the hashing logic lives in one place
    // instead of being a private field on AuthController.
    public interface IPasswordHashService
    {
        string HashPassword(User user, string plainPassword);

        bool VerifyPassword(User user, string plainPassword);
    }
}
