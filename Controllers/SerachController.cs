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
        [HttpGet("restaurants")]
        public async Task<IActionResult> SearchRestaurants([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Search query is required");

            query = query.ToLower();

            var results = await _context.Restaurants
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

            return Ok(results);

        }
    }
    }
