using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models
{
    // Matches the Users table from the Part 1 SQL script.
    // Role is kept as an enum in code but stored as a string in the DB
    // so it lines up with the CHECK (Role IN ('Organiser','Participant')) constraint.
    public enum UserRole
    {
        Organiser,
        Participant
    }

    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        // Never store the raw password - only the hash produced by PasswordHasher<User>.
        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Set once the user uploads a profile picture through /api/users/me/profile-picture.
        // Points at a blob in Azure Storage - the MVC app never talks to Azure directly.
        [MaxLength(500)]
        public string? ProfilePictureUrl { get; set; }

        // Navigation properties
        public ICollection<Event> EventsOrganised { get; set; } = new List<Event>();
        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
}
