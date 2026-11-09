using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs.Categories;

namespace RaceDay.API.Controllers
{
    /// <summary>
    /// Editing/removing a single category by its own id. Adding a category happens
    /// through EventsController (POST /api/events/{id}/categories) since a category
    /// can't exist without an event.
    /// </summary>
    [Route("api/categories")]
    public class CategoriesController : BaseApiController
    {
        private readonly RaceDayContext _context;

        public CategoriesController(RaceDayContext context)
        {
            _context = context;
        }

        /// <summary>Updates a category. Only the Organiser who owns the parent event can do this.</summary>
        /// <response code="200">Category updated.</response>
        /// <response code="403">Not the owner.</response>
        /// <response code="404">Category not found.</response>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCategory(int id, CreateCategoryDto dto)
        {
            var check = RequireRole("Organiser");
            if (check != null) return check;

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null)
            {
                return NotFound(new { message = "Category not found." });
            }

            if (category.Event!.OrganiserID != CurrentUserId!.Value)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You can only manage categories for your own events." });
            }

            category.Name = dto.Name;
            category.EntryFee = dto.EntryFee;
            category.MaxParticipants = dto.MaxParticipants;
            await _context.SaveChangesAsync();

            return Ok(new CategoryDto
            {
                CategoryID = category.CategoryID,
                EventID = category.EventID,
                Name = category.Name,
                EntryFee = category.EntryFee,
                MaxParticipants = category.MaxParticipants
            });
        }

        /// <summary>Removes a category from an event. Organiser-owner only.</summary>
        /// <response code="204">Deleted.</response>
        /// <response code="403">Not the owner.</response>
        /// <response code="404">Category not found.</response>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var check = RequireRole("Organiser");
            if (check != null) return check;

            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null)
            {
                return NotFound(new { message = "Category not found." });
            }

            if (category.Event!.OrganiserID != CurrentUserId!.Value)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You can only manage categories for your own events." });
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
