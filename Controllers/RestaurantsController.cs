using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Code_Curry.Models;

namespace Code_Curry.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RestaurantsController : ControllerBase
    {
        private readonly CodeCurryContext _context;

        public RestaurantsController(CodeCurryContext context)
        {
            _context = context;
        }

        // GET: api/Restaurants/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Restaurant>> GetRestaurant(int id)
        {
            var restaurant = await _context.Restaurants.FindAsync(id);
            if (restaurant == null)
                return NotFound();

            return restaurant;
        }

        // PUT: api/Restaurants/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRestaurant(int id, Restaurant restaurant)
        {
            if (id != restaurant.RestId)
                return BadRequest();

            _context.Entry(restaurant).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Restaurants.Any(e => e.RestId == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/Restaurants/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRestaurant(int id)
        {
            var restaurant = await _context.Restaurants.FindAsync(id);
            if (restaurant == null)
                return NotFound();

            _context.Restaurants.Remove(restaurant);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PATCH: api/Restaurants/5/status?isOpen=true
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateOpenStatus(int id, [FromQuery] bool isOpen)
        {
            var restaurant = await _context.Restaurants.FindAsync(id);
            if (restaurant == null)
                return NotFound();

            restaurant.IsOpen = isOpen;
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Restaurant {(isOpen ? "opened" : "closed")} successfully." });
        }
    }
}
