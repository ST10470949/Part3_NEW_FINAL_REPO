using System.ComponentModel.DataAnnotations;

namespace RaceDay.MVC.Models
{
    public class ResultViewModel
    {
        public int ResultID { get; set; }
        public int EnrolmentID { get; set; }
        public string? ParticipantName { get; set; }
        public string? EventName { get; set; }
        public DateTime EventDate { get; set; }
        public string? CategoryName { get; set; }
        public TimeSpan? FinishTime { get; set; }
        public int? Position { get; set; }
        public int TotalFinishersInCategory { get; set; }

        // "47th out of 312 finishers" - built here once instead of repeating the
        // ordinal-suffix logic inside the Razor view.
        public string PositionDisplay
        {
            get
            {
                if (Position == null)
                {
                    return "Not yet published";
                }

                return TotalFinishersInCategory > 0
                    ? $"{Ordinal(Position.Value)} out of {TotalFinishersInCategory} finishers"
                    : Ordinal(Position.Value);
            }
        }

        private static string Ordinal(int number)
        {
            if (number % 100 is 11 or 12 or 13)
            {
                return $"{number}th";
            }

            return (number % 10) switch
            {
                1 => $"{number}st",
                2 => $"{number}nd",
                3 => $"{number}rd",
                _ => $"{number}th"
            };
        }
    }

    // Used on the Organiser's Results page - one of these per un-captured enrolment.
    public class CaptureResultViewModel
    {
        public int EnrolmentID { get; set; }
        public string? ParticipantName { get; set; }
        public string? CategoryName { get; set; }
        public bool AlreadyCaptured { get; set; }

        [Display(Name = "Finish time (hh:mm:ss)")]
        [DataType(DataType.Duration)]
        public TimeSpan? FinishTime { get; set; }

        [Range(1, 100000, ErrorMessage = "Position must be a positive number.")]
        public int? Position { get; set; }
    }
}
