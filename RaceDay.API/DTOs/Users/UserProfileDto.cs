namespace RaceDay.API.DTOs.Users
{
    public class UserProfileDto
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }
}
