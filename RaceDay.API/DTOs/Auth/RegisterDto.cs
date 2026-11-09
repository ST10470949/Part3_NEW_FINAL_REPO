using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs.Auth
{
    public class RegisterDto
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string Password { get; set; } = string.Empty;

        // Expected to be "Organiser" or "Participant" - checked manually in the controller
        // so we can return a friendly error instead of a generic model-binding failure.
        [Required]
        public string Role { get; set; } = string.Empty;
    }
}
