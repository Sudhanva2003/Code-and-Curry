using Code_Curry.DTOs;
using Code_Curry.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Code_Curry.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RestaurantController : ControllerBase
    {
        private readonly CodeCurryContext _context;

        public RestaurantController(CodeCurryContext context)
        {
            _context = context;
        }

        // Register a new restaurant
        [HttpPost("RegisterRestaurant")]
        public async Task<IActionResult> RegisterRestaurant([FromBody] RestaurantDto dto)
        {
            // Check if email exists in Restaurants or Users table
            bool emailExists = await _context.Restaurants.AnyAsync(r => r.Email == dto.Email);
            bool userEmailExists = await _context.Users.AnyAsync(u => u.Email == dto.Email);
            if (emailExists || userEmailExists)
                return Conflict("Email already exists."); // 409 Conflict

            // Hash the password
            var hashedPassword = HashPassword(dto.Password);

            var restaurant = new Restaurant
            {
                Name = dto.Name,
                Address = dto.Address,
                Email = dto.Email,
                Phone = dto.Phone,
                Cuisine = dto.Cuisine,
                PasswordHash = hashedPassword,
                GstNo = dto.GstNo,
                FssaiNo = dto.FssaiNo,
                RestStatus = dto.RestStatus,

                RestImageUrl = string.IsNullOrWhiteSpace(dto.RestImageUrl)
                    ? "https://t3.ftcdn.net/jpg/03/24/73/92/360_F_324739203_keeq8udvv0P2h1MLYJ0GLSlTBagoXS48.jpg"
                    : dto.RestImageUrl
            };

            await _context.Restaurants.AddAsync(restaurant);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                restaurant.RestId,
                restaurant.Name,
                restaurant.Address,
                restaurant.Email,
                restaurant.RestStatus,
                restaurant.RestImageUrl,
                restaurant.GstNo,
                restaurant.FssaiNo
            });
        }

        // Edit restaurant details
        [HttpPut("EditRestaurant/{RestId}")]
        public async Task<IActionResult> EditRestaurant(int RestId, [FromBody] RestaurantEditDto dto)
        {
            var restaurant = await _context.Restaurants.FindAsync(RestId);
            if (restaurant == null) return NotFound("Restaurant not found");

            restaurant.Name = dto.Name;
            restaurant.Address = dto.Address;
            restaurant.Phone = dto.Phone;
            restaurant.RestStatus = dto.RestStatus;  // Resolved conflict here

            if (!string.IsNullOrWhiteSpace(dto.RestImageUrl))
                restaurant.RestImageUrl = dto.RestImageUrl;

            if (!string.IsNullOrWhiteSpace(dto.Cuisine))
                restaurant.Cuisine = dto.Cuisine;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Restaurant updated successfully",
                restaurant.RestStatus,  // Resolved conflict here
                restaurant.RestImageUrl
            });
        }

        // View restaurant profile
        [HttpGet("ViewRestaurant/{RestId}")]
        public async Task<IActionResult> ViewRestaurant(int RestId)
        {
            var restaurant = await _context.Restaurants.FirstOrDefaultAsync(r => r.RestId == RestId);
            if (restaurant == null) return NotFound("Restaurant not found");

            var restaurantDetails = new RestaurantProfileDto
            {
                Name = restaurant.Name,
                Address = restaurant.Address,
                Rating = restaurant.Rating,
                Phone = restaurant.Phone,
                Email = restaurant.Email,
                RestStatus = restaurant.RestStatus,
                RestImageUrl = restaurant.RestImageUrl,
                GstNo = restaurant.GstNo,
                FssaiNo = restaurant.FssaiNo,
                Cuisine = restaurant.Cuisine
            };

            return Ok(restaurantDetails);
        }

        // Get restaurant menu
        [HttpGet("Menu/{RestId}")]
        public async Task<IActionResult> ViewMenu(int RestId)
        {
            var restaurant = await _context.Restaurants
                .Include(r => r.Foods)
                .FirstOrDefaultAsync(r => r.RestId == RestId);

            if (restaurant == null) return NotFound("Restaurant not found");

            // Only include foods that are not deleted
            var menu = restaurant.Foods
                .Where(f => f.FoodStatus != "Deleted")
                .Select(f => new MenuItemDto
                {
                    FoodId = f.FoodId,
                    Name = f.Name,
                    Category = f.Category,
                    Description = f.Description,
                    Price = f.Price,
                    FoodStatus = f.FoodStatus,
                    FoodImageUrl = f.FoodImageUrl
                })
                .ToList();

            return Ok(menu);
        }

        // Get restaurants by rating (Home page)
        [HttpGet("Home")]
        public async Task<IActionResult> GetRestaurantsByRating()
        {
            var restaurants = await _context.Restaurants
                .Where(r => r.RestStatus == "Open" || r.RestStatus == "Closed")
                .OrderByDescending(r => r.Rating)
                .Select(r => new RestaurantSummaryDto
                {
                    RestId = r.RestId,
                    Name = r.Name,
                    Rating = r.Rating,
                    RestStatus = r.RestStatus,
                    RestImageUrl = r.RestImageUrl
                })
                .ToListAsync();

            return Ok(restaurants);
        }

        // View restaurant open orders
        [HttpGet("ViewRestaurantOpenOrders/{RestId}")]
        public async Task<IActionResult> ViewRestaurantOpenOrders(int RestId)
        {
            var orders = await _context.Orders
                .Where(o => o.RestId == RestId && o.Status == "Paid" && o.User.UserStatus == "Active")
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Food)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new
                {
                    o.OrderId,
                    o.OrderDate,
                    o.Status,
                    o.TotalAmount,
                    Items = o.OrderDetails.Select(od => new
                    {
                        od.Food.Name,
                        od.Quantity,
                        od.Food.FoodImageUrl
                    }).ToList()
                })
                .ToListAsync();

            return Ok(orders);
        }

        // View restaurant past orders
        [HttpGet("ViewRestaurantPastOrders/{RestId}")]
        public async Task<IActionResult> ViewRestaurantPastOrders(int RestId)
        {
            var orders = await _context.Orders
                .Where(o => o.RestId == RestId && o.Status == "Prepared" || o.Status == "Delivered")
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Food)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new
                {
                    o.OrderId,
                    o.OrderDate,
                    o.Status,
                    o.TotalAmount,
                    o.Discount,
                    o.HandlingFee,
                    o.GST,
                    FinalPrice = o.TotalAmount - o.Discount + o.GST + o.HandlingFee,
                    Items = o.OrderDetails.Select(od => new
                    {
                        od.Food.Name,
                        od.Quantity,
                        od.Food.FoodImageUrl
                    }).ToList()
                })
                .ToListAsync();

            return Ok(orders);
        }

        // Search restaurants
        [HttpGet("Search")]
        public async Task<IActionResult> SearchRestaurants([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Search term is required.");

            var matchingRestaurants = await _context.Restaurants
                .Where(r => r.Name.Contains(name))
                .Select(r => new RestaurantSummaryDto
                {
                    RestId = r.RestId,
                    Name = r.Name,
                    Rating = r.Rating,
                    RestImageUrl = r.RestImageUrl,
                    RestStatus = r.RestStatus
                })
                .ToListAsync();

            return Ok(matchingRestaurants);
        }

        // Mark order as prepared
        [HttpPut("Prepared/{orderId}")]
        public async Task<IActionResult> MarkOrderPrepared(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return NotFound("Order not found");

            order.Status = "Prepared";
            await _context.SaveChangesAsync();
            return Ok("Order marked as prepared");
        }

        // Delete restaurant (soft delete)
        [HttpDelete("DeleteRestaurant/{RestId}")]
        public async Task<IActionResult> DeleteRestaurant(int RestId)
        {
            var restaurant = await _context.Restaurants.FindAsync(RestId);
            if (restaurant == null)
                return NotFound(new { message = "Restaurant not found" });

            restaurant.RestStatus = "Deleted";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Restaurant marked as deleted" });
        }

        // Helper method to hash passwords
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        // ? Toggle restaurant open/close status
        [HttpPatch("ChangeAvailability/{restId}")]
        public async Task<IActionResult> ChangeRestaurantAvailability(int restId)
        {
            try
            {
                var restaurant = await _context.Restaurants.FindAsync(restId);
                if (restaurant == null)
                    return NotFound(new { message = "Restaurant not found" });

                restaurant.RestStatus = restaurant.RestStatus == "Open" ? "Closed" : "Open";
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = $"Restaurant {(restaurant.RestStatus == "Open" ? "opened" : "closed")} successfully",
                    restId = restaurant.RestId,
                    name = restaurant.Name,
                    restStatus = restaurant.RestStatus
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error updating restaurant availability", error = ex.Message });
            }
        }
    }
}
