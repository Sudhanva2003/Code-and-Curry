using Code_Curry.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        [Authorize(Roles = "Admin,Customer")]
        [HttpGet("search")]
        public async Task<IActionResult> SearchAll([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Search query is required");

            query = query.Trim().ToLower();

            var matchedRestaurants = await _context.Restaurants
                .Where(r =>
                    EF.Functions.Like(r.Name.ToLower(), $"%{query}%") ||
                    EF.Functions.Like(r.Address.ToLower(), $"%{query}%") ||
                    (!string.IsNullOrEmpty(r.Cuisine) && EF.Functions.Like(r.Cuisine.ToLower(), $"%{query}%"))
                )
                .Select(r => new DTOs.SearchDto
                {
                    RestId = r.RestId,
                    Name = r.Name,
                    Address = r.Address,
                    Rating = r.Rating,
                    RestStatus = r.RestStatus
                })
                .ToListAsync();

            var matchedFoods = await _context.Foods
                .Where(f =>
                    EF.Functions.Like(f.Name.ToLower(), $"%{query}%") ||
                    EF.Functions.Like(f.Description.ToLower(), $"%{query}%") ||
                    EF.Functions.Like(f.Category.ToLower(), $"%{query}%")
                )
                .Select(f => new DTOs.FoodSearchDto
                {
                    FoodId = f.FoodId,
                    RestId = f.RestId,
                    Name = f.Name,
                    Description = f.Description,
                    Category = f.Category,
                    Price = f.Price,
                    FoodStatus = f.FoodStatus
                })
                .ToListAsync();

            var result = new
            {
                Restaurants = matchedRestaurants,
                Foods = matchedFoods
            };

            return Ok(result);
        }
        [Authorize(Roles = "Admin,Customer")]
        [HttpGet("Foods")]
        public async Task<IActionResult> GetFoods([FromQuery] int restId, [FromQuery] string category = "none", [FromQuery] string sort = "none")
        {
            var query = _context.Foods.AsQueryable();

            // Filter by restaurant
            query = query.Where(f => f.RestId == restId);

            // Filter by category
            if (!string.IsNullOrEmpty(category) && category.ToLower() != "none")
            {
                query = query.Where(f => f.Category.ToLower() == category.ToLower());
            }

            // Sort
            if (!string.IsNullOrEmpty(sort))
            {
                switch (sort.ToLower())
                {
                    case "price":
                        query = query.OrderBy(f => f.Price);
                        break;
                    // You can add more sorting options here later
                    default:
                        query = query.OrderBy(f => f.FoodId); // default sort
                        break;
                }
            }

            var foods = await query.ToListAsync();
            return Ok(foods);
        }

        // GET: api/Filter/Restaurants?sort=rating
        [Authorize(Roles = "Admin,Customer")]
        [HttpGet("Restaurants")]
        public async Task<IActionResult> GetRestaurants([FromQuery] string sort = "rating")
        {
            var query = _context.Restaurants.AsQueryable();

            if (!string.IsNullOrEmpty(sort))
            {
                switch (sort.ToLower())
                {
                    case "rating":
                        query = query.OrderByDescending(r => r.Rating);
                        break;
                    case "distance":
                        // For now, distance sorting logic can be added later
                        query = query.OrderBy(r => r.RestId); // placeholder
                        break;
                }
            }

            var restaurants = await query.ToListAsync();
            return Ok(restaurants);
        }
        [Authorize(Roles = "Admin,Customer")]
        [HttpGet("SearchByCuisine")]
        public async Task<IActionResult> SearchByCuisine([FromQuery] string cuisine)
        {
            if (string.IsNullOrWhiteSpace(cuisine))
                return BadRequest("Cuisine is required");

            cuisine = cuisine.Trim().ToLower();

            var matchedRestaurants = await _context.Restaurants
                .Where(r => !string.IsNullOrEmpty(r.Cuisine) && r.Cuisine.ToLower().Contains(cuisine))
                .Select(r => new DTOs.SearchDto
                {
                    RestId = r.RestId,
                    Name = r.Name,
                    Address = r.Address,
                    Rating = r.Rating,
                    RestStatus = r.RestStatus,
                    RestImageUrl = r.RestImageUrl,
                    Cuisine = r.Cuisine
                })
                .ToListAsync();

            return Ok(matchedRestaurants);
        }

    }
}