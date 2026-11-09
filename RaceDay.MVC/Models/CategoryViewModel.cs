using System.ComponentModel.DataAnnotations;

namespace RaceDay.MVC.Models
{
    public class CategoryViewModel
    {
        public int CategoryID { get; set; }
        public int EventID { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal EntryFee { get; set; }
        public int MaxParticipants { get; set; }
    }

    public class CreateCategoryViewModel
    {
        public int EventID { get; set; }
        public string? EventName { get; set; }

        [Required(ErrorMessage = "Give the category a name, e.g. '10km' or 'Under 20'.")]
        public string Name { get; set; } = string.Empty;

        [Range(0, 100000, ErrorMessage = "Entry fee can't be negative.")]
        [Display(Name = "Entry fee (R)")]
        public decimal EntryFee { get; set; }

        [Range(1, 100000, ErrorMessage = "There must be room for at least 1 participant.")]
        [Display(Name = "Maximum participants")]
        public int MaxParticipants { get; set; } = 100;

        public List<CategoryViewModel> ExistingCategories { get; set; } = new();
    }
}
