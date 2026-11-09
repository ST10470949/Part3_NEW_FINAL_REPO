using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RaceDay.MVC.Models
{
    public class ProfileViewModel
    {
        public int UserID { get; set; }

        [Required(ErrorMessage = "Full name can't be empty.")]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email can't be empty.")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? ProfilePictureUrl { get; set; }

        [Display(Name = "New profile picture")]
        public IFormFile? ProfilePictureFile { get; set; }
    }
}
