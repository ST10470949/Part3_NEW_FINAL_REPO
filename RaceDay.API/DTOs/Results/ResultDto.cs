namespace RaceDay.API.DTOs.Results
{
    public class ResultDto
    {
        public int ResultID { get; set; }
        public int EnrolmentID { get; set; }
        public string? ParticipantName { get; set; }
        public string? EventName { get; set; }
        public DateTime EventDate { get; set; }
        public string? CategoryName { get; set; }
        public TimeSpan? FinishTime { get; set; }
        public int? Position { get; set; }

        // How many finishers were recorded in the same category - lets the Participant
        // see "47th out of 312 finishers" instead of just a bare number.
        public int TotalFinishersInCategory { get; set; }
    }
}
