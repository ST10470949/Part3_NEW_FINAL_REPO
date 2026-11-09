using Microsoft.AspNetCore.Identity;
using RaceDay.API.Models;

namespace RaceDay.API.Services
{
    // Uses Microsoft.AspNetCore.Identity's PasswordHasher under the hood - it already
    // does salting + a proper hashing algorithm (PBKDF2), so there's no reason to
    // hand-roll our own.
    public class PasswordHashService : IPasswordHashService
    {
        private readonly PasswordHasher<User> _hasher = new();

        public string HashPassword(User user, string plainPassword)
        {
            return _hasher.HashPassword(user, plainPassword);
        }

        public bool VerifyPassword(User user, string plainPassword)
        {
            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, plainPassword);
            return result != PasswordVerificationResult.Failed;
        }
    }
}
