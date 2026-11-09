using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public enum EnrolmentStatus
    {
        Pending,
        Confirmed,
        Cancelled
    }

    // Links a Participant to a Category. This is the join between "who" and "what race".
    public class Enrolment
    {
        [Key]
        public int EnrolmentID { get; set; }

        [Required]
        public int ParticipantID { get; set; }

        [Required]
        public int CategoryID { get; set; }

        public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;

        public EnrolmentStatus Status { get; set; } = EnrolmentStatus.Confirmed;

        [ForeignKey(nameof(ParticipantID))]
        public User? Participant { get; set; }

        [ForeignKey(nameof(CategoryID))]
        public Category? Category { get; set; }

        public Result? Result { get; set; }
    }
}
