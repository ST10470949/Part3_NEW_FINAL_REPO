using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs.Results
{
    public class CreateResultDto
    {
        [Required]
        public int EnrolmentId { get; set; }

        // Sent as "hh:mm:ss", e.g. "01:45:30"
        public TimeSpan? FinishTime { get; set; }

        public int? Position { get; set; }
    }
}
