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
            restaurant.RestStatus = dto.RestStatus;


            if (!string.IsNullOrWhiteSpace(dto.RestImageUrl))
                restaurant.RestImageUrl = dto.RestImageUrl;

            if (!string.IsNullOrWhiteSpace(dto.Cuisine))
                restaurant.Cuisine = dto.Cuisine;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Restaurant updated successfully",
                restaurant.RestStatus

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
                FssaiNo = restaurant.FssaiNo
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
                .Where(f => f.FoodStatus != "Deleted")   // <-- filter out deleted
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

        [HttpGet("ViewRestaurantOpenOrders/{RestId}")]
        public async Task<IActionResult> ViewRestaurantOpenOrders(int RestId)
        {
            var orders = await _context.Orders
               .Where(o => o.Status == "Paid"||
               o.Status=="Assigned")

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

        [HttpGet("ViewRestaurantPastOrders/{RestId}")]
        public async Task<IActionResult> ViewRestaurantPastOrders(int RestId)
        {
            var orders = await _context.Orders
               .Where(o => o.Status == "Delivered"
         || o.Status == "Prepared"
         || o.Status == "CancelledByRest"
         || o.Status == "CancelledByCustomer"
         || o.Status=="CancelledByDeliverer")

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
                    o.Gst,
                    FinalPrice = o.TotalAmount - o.Discount + o.Gst + o.HandlingFee,
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

        [HttpGet("Search")]
        public async Task<IActionResult> SearchRestaurants([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Search term is required.");

            var matchingRestaurants = await _context.Restaurants
                .Where(r => r.Name.Contains(name)) // case-sensitive
                                                   //.Where(r => EF.Functions.Like(r.Name, $"%{name}%")) // case-insensitive for SQL Server
                .Select(r => new RestaurantSummaryDto
                {
                    RestId = r.RestId,
                    Name = r.Name,
                    Rating = r.Rating,
                    //IsOpen = r.IsOpen,
                    RestImageUrl = r.RestImageUrl
                })
                .ToListAsync();

            return Ok(matchingRestaurants);
        }


        [HttpPut("Prepared/{orderId}")]
        public async Task<IActionResult> MarkOrderPrepared(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return NotFound("Order not found");

            order.Status = "Prepared";
            await _context.SaveChangesAsync();
            return Ok("Order marked as prepared");
        }

        [HttpPatch("SetRestaurantStatus/{RestId}")]
        public async Task<IActionResult> SetRestaurantStatus(int RestId, [FromQuery] string action)
        {
            var restaurant = await _context.Restaurants.FindAsync(RestId);
            if (restaurant == null) return NotFound("Restaurant not found.");

            action = action?.ToLower();
            if (action != "open" && action != "close")
                return BadRequest("Invalid action. Use 'open' or 'close'.");

            restaurant.RestStatus = action == "open" ? "Open" : "Closed";
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Restaurant is now {restaurant.RestStatus}.", restaurant.RestStatus });
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

        [HttpPut("CancelOrder/{orderId}")]
        public async Task<IActionResult> CancelOrderByRestaurant(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return NotFound();

            order.Status = "CancelledByRest";
            await _context.SaveChangesAsync();
            return Ok("Order cancelled by restaurant.");
        }


}
}
