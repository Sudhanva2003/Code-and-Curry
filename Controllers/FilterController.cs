using Code_Curry.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace Code_Curry.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class FilterController : ControllerBase
    {
        private readonly CodeCurryContext _context;

        public FilterController(CodeCurryContext context)
        {
            _context = context;
        }
        [HttpGet("search")]
        public async Task<IActionResult> SearchAll([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Search query is required");

            query = query.ToLower();

            var matchedRestaurants = await _context.Restaurants
                .Where(r => r.Name.ToLower().Contains(query) || r.Address.ToLower().Contains(query))
                .Select(r => new DTOs.SearchDto
                {
                    RestId = r.RestId,
                    Name = r.Name,
                    Address = r.Address,
                    Rating = r.Rating,
                    IsOpen = r.IsOpen
                })
                .ToListAsync();

            var matchedFoods = await _context.Foods
                .Where(f => f.Name.ToLower().Contains(query) || f.Description.ToLower().Contains(query) || f.Category.ToLower().Contains(query))
                .Select(f => new DTOs.FoodSearchDto
                {
                    FoodId = f.FoodId,
                    RestId = f.RestId,
                    Name = f.Name,
                    Description = f.Description,
                    Category = f.Category,
                    Price = f.Price,
                    IsAvailable = f.IsAvailable
                })
                .ToListAsync();

            var result = new
            {
                Restaurants = matchedRestaurants,
                Foods = matchedFoods
            };

            return Ok(result);
        }
    }
}
