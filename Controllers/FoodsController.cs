using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Code_Curry.Models;
using Code_Curry.Dtos;
using System.Linq;
using System.Threading.Tasks;

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

       
        [HttpPost("AddFood")]
        public async Task<ActionResult<FoodResponseDto>> AddFood([FromBody] FoodCreateDto dto)
        {
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
                IsAvailable = dto.IsAvailable,
                FoodImageUrl = string.IsNullOrWhiteSpace(dto.FoodImageUrl)
                    ? "https://static.vecteezy.com/system/resources/previews/004/204/922/non_2x/food-logo-template-design-icon-illustration-vector.jpg"
                    : dto.FoodImageUrl
            };

            _context.Foods.Add(food);
            await _context.SaveChangesAsync();

            var foodDto = new FoodResponseDto
            {
                FoodId = food.FoodId,
                RestId = food.RestId,
                Name = food.Name,
                Description = food.Description,
                Price = food.Price,
                Category = food.Category,
                IsAvailable = food.IsAvailable,
                FoodImageUrl = food.FoodImageUrl,
                RestaurantName = rest.Name
            };

            return CreatedAtAction(nameof(GetFood), new { FoodId = food.FoodId }, foodDto);
        }

      
        [HttpGet("GetFood/{FoodId}")]
        public async Task<ActionResult<FoodResponseDto>> GetFood(int FoodId)
        {
            var food = await _context.Foods
                .Include(f => f.Rest)
                .FirstOrDefaultAsync(f => f.FoodId == FoodId);

            if (food == null) return NotFound();

            return new FoodResponseDto
            {
                FoodId = food.FoodId,
                RestId = food.RestId,
                Name = food.Name,
                Description = food.Description,
                Price = food.Price,
                Category = food.Category,
                IsAvailable = food.IsAvailable,
                FoodImageUrl = food.FoodImageUrl,
                RestaurantName = food.Rest.Name
            };
        }

      
        [HttpPut("UpdateFood/{FoodId}")]
        public async Task<IActionResult> UpdateFood(int FoodId, [FromBody] FoodUpdateDto dto)
        {
            var food = await _context.Foods.FindAsync(FoodId);
            if (food == null) return NotFound();

            food.Name = dto.Name;
            food.Description = dto.Description;
            food.Price = dto.Price;
            food.Category = dto.Category;
            food.IsAvailable = dto.IsAvailable;

            // Optional update for image URL
            if (!string.IsNullOrWhiteSpace(dto.FoodImageUrl))
                food.FoodImageUrl = dto.FoodImageUrl;

            await _context.SaveChangesAsync();
            return NoContent();
        }

      
        [HttpDelete("DeleteFood/{id}")]
        public async Task<IActionResult> DeleteFood(int id)
        {
            var food = await _context.Foods.FindAsync(id);
            if (food == null) return NotFound();

            var orderDetails = _context.OrderDetails.Where(od => od.FoodId == id);
            _context.OrderDetails.RemoveRange(orderDetails);

            _context.Foods.Remove(food);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Food deleted successfully" });
        }

       
        [HttpPatch("ChangeAvailability/{FoodId}")]
        public async Task<IActionResult> SetAvailability(int FoodId, [FromBody] FoodAvailabilityDto dto)
        {
            var food = await _context.Foods.FindAsync(FoodId);
            if (food == null) return NotFound();

            food.IsAvailable = dto.IsAvailable;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("GetRestaurantFoods/{RestId}")]
        public async Task<IActionResult> GetRestaurantFoods(int RestId)
        {
            var restaurant = await _context.Restaurants
                .Include(r => r.Foods)
                .FirstOrDefaultAsync(r => r.RestId == RestId);

            if (restaurant == null) return NotFound();

            var foods = restaurant.Foods.Select(f => new FoodResponseDto
            {
                FoodId = f.FoodId,
                RestId = f.RestId,
                Name = f.Name,
                Description = f.Description,
                Price = f.Price,
                Category = f.Category,
                IsAvailable = f.IsAvailable,
                FoodImageUrl = f.FoodImageUrl,
                RestaurantName = restaurant.Name
            }).ToList();

            return Ok(foods);
        }
    }
}
