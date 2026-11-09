using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    // Captured by an Organiser once an Enrolment's race is finished.
    public class Result
    {
        [Key]
        public int ResultID { get; set; }

        [Required]
        public int EnrolmentID { get; set; }

        [Required]
        public int CapturedByID { get; set; }

        public TimeSpan? FinishTime { get; set; }

        public int? Position { get; set; }

        [ForeignKey(nameof(EnrolmentID))]
        public Enrolment? Enrolment { get; set; }

        [ForeignKey(nameof(CapturedByID))]
        public User? CapturedBy { get; set; }
    }
}
