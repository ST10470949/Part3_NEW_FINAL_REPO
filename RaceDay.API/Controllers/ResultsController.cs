using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs.Results;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    /// <summary>
    /// Results are captured by the Organiser after race day and viewed privately
    /// by the Participant they belong to.
    /// </summary>
    [Route("api/results")]
    public class ResultsController : BaseApiController
    {
        private readonly RaceDayContext _context;

        public ResultsController(RaceDayContext context)
        {
            _context = context;
        }

        /// <summary>Captures a finishing position/time for a completed enrolment.</summary>
        /// <response code="201">Result captured.</response>
        /// <response code="403">Not the Organiser who owns the event this enrolment belongs to.</response>
        /// <response code="404">Enrolment doesn't exist.</response>
        /// <response code="409">A result was already captured for this enrolment.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CaptureResult(CreateResultDto dto)
        {
            var check = RequireRole("Organiser");
            if (check != null) return check;

            var enrolment = await _context.Enrolments
                .Include(e => e.Category).ThenInclude(c => c!.Event)
                .Include(e => e.Participant)
                .FirstOrDefaultAsync(e => e.EnrolmentID == dto.EnrolmentId);

            if (enrolment == null)
            {
                return NotFound(new { message = "That enrolment doesn't exist." });
            }

            if (enrolment.Category!.Event!.OrganiserID != CurrentUserId!.Value)
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "You can only capture results for your own events." });
            }

            var alreadyCaptured = await _context.Results.AnyAsync(r => r.EnrolmentID == dto.EnrolmentId);
            if (alreadyCaptured)
            {
                return Conflict(new { message = "A result has already been captured for this enrolment." });
            }

            var result = new Result
            {
                EnrolmentID = dto.EnrolmentId,
                CapturedByID = CurrentUserId!.Value,
                FinishTime = dto.FinishTime,
                Position = dto.Position
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return StatusCode(StatusCodes.Status201Created, new ResultDto
            {
                ResultID = result.ResultID,
                EnrolmentID = result.EnrolmentID,
                ParticipantName = enrolment.Participant?.FullName,
                EventName = enrolment.Category.Event.Name,
                EventDate = enrolment.Category.Event.EventDate,
                CategoryName = enrolment.Category.Name,
                FinishTime = result.FinishTime,
                Position = result.Position
            });
        }

        /// <summary>Lists the logged-in Participant's own historical results.</summary>
        /// <response code="200">Array of results.</response>
        /// <response code="403">Logged in, but not a Participant.</response>
        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetMyResults()
        {
            var check = RequireRole("Participant");
            if (check != null) return check;

            var results = await _context.Results
                .Include(r => r.Enrolment).ThenInclude(e => e!.Category).ThenInclude(c => c!.Event)
                .Where(r => r.Enrolment!.ParticipantID == CurrentUserId!.Value)
                .ToListAsync();

            // Done as a second, simple query rather than trying to force a per-category
            // count into the EF query above - a lot easier to read and this table will
            // never be huge enough for it to matter.
            var finisherCountsByCategory = await _context.Results
                .Where(r => r.Position != null)
                .GroupBy(r => r.Enrolment!.CategoryID)
                .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.CategoryId, g => g.Count);

            var dtos = results.Select(r => new ResultDto
            {
                ResultID = r.ResultID,
                EnrolmentID = r.EnrolmentID,
                EventName = r.Enrolment!.Category!.Event!.Name,
                EventDate = r.Enrolment.Category.Event.EventDate,
                CategoryName = r.Enrolment.Category.Name,
                FinishTime = r.FinishTime,
                Position = r.Position,
                TotalFinishersInCategory = finisherCountsByCategory.GetValueOrDefault(r.Enrolment.CategoryID, 0)
            }).ToList();

            // A Participant should only ever see their own results - the filter above
            // (ParticipantID == CurrentUserId) is what enforces that, not the route.
            return Ok(dtos);
        }
    }
}
