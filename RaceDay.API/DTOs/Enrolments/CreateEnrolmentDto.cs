using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs.Enrolments
{
    public class CreateEnrolmentDto
    {
        [Required]
        public int CategoryId { get; set; }
    }
}
