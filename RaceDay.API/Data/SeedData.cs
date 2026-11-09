using RaceDay.API.Models;
using RaceDay.API.Services;

namespace RaceDay.API.Data
{
    // Puts a small set of demo accounts into the database the first time the API
    // runs against an empty database, so there's always something to log in with
    // when marking/demoing the app. Only runs if the Users table is empty, so it
    // won't touch a database that already has real accounts in it.
    public static class SeedData
    {
        // Same password for every seeded account - fine for a demo/marking environment,
        // never do this in a real production system.
        public const string DemoPassword = "RaceDay2026!";

        public static async Task SeedAsync(RaceDayContext context, IPasswordHashService passwordHashService)
        {
            if (context.Users.Any())
            {
                return;
            }

            var demoUsers = new List<User>
            {
                new()
                {
                    FullName = "Abongile Nohayi",
                    Email = "abongile.nohayi@raceday.co.za",
                    Role = UserRole.Organiser
                },
                new()
                {
                    FullName = "Nomhle Manqana",
                    Email = "nomhle.manqana@raceday.co.za",
                    Role = UserRole.Organiser
                },
                new()
                {
                    FullName = "Owen Mkhize",
                    Email = "owen.mkhize@example.com",
                    Role = UserRole.Participant
                },
                new()
                {
                    FullName = "Andile Maku",
                    Email = "andile.maku@example.com",
                    Role = UserRole.Participant
                }
            };

            foreach (var user in demoUsers)
            {
                user.PasswordHash = passwordHashService.HashPassword(user, DemoPassword);
            }

            context.Users.AddRange(demoUsers);
            await context.SaveChangesAsync();
        }
    }
}
