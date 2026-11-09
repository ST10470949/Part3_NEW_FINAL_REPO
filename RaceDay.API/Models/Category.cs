using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    // e.g. "10km Fun Run" or "21km" under a specific Event.
    public class Category
    {
        [Key]
        public int CategoryID { get; set; }

        [Required]
        public int EventID { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "decimal(8,2)")]
        public decimal EntryFee { get; set; }

        public int MaxParticipants { get; set; } = 100;

        [ForeignKey(nameof(EventID))]
        public Event? Event { get; set; }

        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
}
