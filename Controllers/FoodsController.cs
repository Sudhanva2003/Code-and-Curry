using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Code_Curry.Models;
using Code_Curry.Dtos;

namespace Code_Curry.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FoodsController : ControllerBase
    {
        private readonly CodeCurryContext _context;

        public FoodsController(CodeCurryContext context)
        {
            _context = context;
        }

        // ----------------------------
        // 2) Add food and price
        // ----------------------------
        [HttpPost]
        public async Task<ActionResult<Food>> AddFood([FromBody] FoodCreateDto dto)
        {
            // Verify restaurant exists
            var rest = await _context.Restaurants.FindAsync(dto.RestId);
            if (rest == null)
                return BadRequest($"Restaurant with id {dto.RestId} not found.");

            var food = new Food
            {
                RestId = dto.RestId,
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Category = dto.Category,
                IsAvailable = dto.IsAvailable
            };

            _context.Foods.Add(food);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetFood), new { id = food.FoodId }, food);
        }

        // Helper to retrieve a single food (used by CreatedAtAction)
        [HttpGet("{id}")]
        public async Task<ActionResult<Food>> GetFood(int id)
        {
            var food = await _context.Foods.FindAsync(id);
            if (food == null) return NotFound();
            return food;
        }

        // ----------------------------
        // 3) Edit food and price
        // ----------------------------
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFood(int id, [FromBody] FoodUpdateDto dto)
        {
            var food = await _context.Foods.FindAsync(id);
            if (food == null) return NotFound();

            // You must send all fields here
            food.Name = dto.Name;
            food.Description = dto.Description;
            food.Price = dto.Price;
            food.Category = dto.Category;
            food.IsAvailable = dto.IsAvailable;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ----------------------------
        // 4) Delete food method
        // ----------------------------
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFood(int id)
        {
            var food = await _context.Foods.FindAsync(id);
            if (food == null) return NotFound();

            _context.Foods.Remove(food);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ----------------------------
        // 5) Make food unavailable
        // ----------------------------
        [HttpPatch("{id}/availability")]
        public async Task<IActionResult> SetAvailability(int id, [FromBody] FoodAvailabilityDto dto)
        {
            var food = await _context.Foods.FindAsync(id);
            if (food == null) return NotFound();

            food.IsAvailable = dto.IsAvailable;
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
