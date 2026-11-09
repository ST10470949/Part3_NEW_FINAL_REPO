namespace RaceDay.API.DTOs.Categories
{
    public class CategoryDto
    {
        public int CategoryID { get; set; }
        public int EventID { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal EntryFee { get; set; }
        public int MaxParticipants { get; set; }
    }
}
