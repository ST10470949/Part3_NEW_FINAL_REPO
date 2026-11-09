using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs.Categories
{
    public class CreateCategoryDto
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [Range(0, 100000)]
        public decimal EntryFee { get; set; }

        [Range(1, 100000)]
        public int MaxParticipants { get; set; } = 100;
    }
}
