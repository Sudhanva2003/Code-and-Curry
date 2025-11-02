using Code_Curry.DTOs;
using Code_Curry.Models;
using Microsoft.AspNetCore.Authorization;
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

        [AllowAnonymous]
        [HttpPost("RegisterRestaurant")]
        public async Task<IActionResult> RegisterRestaurant([FromBody] RestaurantDto dto)
        {
            bool emailExists = await _context.Restaurants.AnyAsync(r => r.Email == dto.Email);
            bool userEmailExists = await _context.Users.AnyAsync(u => u.Email == dto.Email);
            if (emailExists || userEmailExists)
                return Conflict("Email already exists.");

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

        [Authorize(Roles = "Admin,Restaurant")]
        [HttpPut("EditRestaurant/{RestId}")]
        public async Task<IActionResult> EditRestaurant(int RestId, [FromBody] RestaurantEditDto dto)
        {
            var restaurant = await _context.Restaurants.FindAsync(RestId);
            if (restaurant == null) return NotFound("Restaurant not found");

            restaurant.Name = dto.Name;
            restaurant.Address = dto.Address;
            restaurant.Phone = dto.Phone;
            

            if (!string.IsNullOrWhiteSpace(dto.RestImageUrl))
                restaurant.RestImageUrl = dto.RestImageUrl;

            if (!string.IsNullOrWhiteSpace(dto.Cuisine))
                restaurant.Cuisine = dto.Cuisine;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Restaurant updated successfully" });
        }

        [Authorize(Roles = "Admin,Restaurant")]
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
                Cuisine=restaurant.Cuisine,
                RestStatus = restaurant.RestStatus,
                RestImageUrl = restaurant.RestImageUrl,
                GstNo = restaurant.GstNo,
                FssaiNo = restaurant.FssaiNo
            };

            return Ok(restaurantDetails);
        }

        [Authorize(Roles = "Admin,Restaurant")]
        [HttpGet("Menu/{RestId}")]
        public async Task<IActionResult> ViewMenu(int RestId)
        {
            var restaurant = await _context.Restaurants
                .Include(r => r.Foods)
                .FirstOrDefaultAsync(r => r.RestId == RestId);

            if (restaurant == null) return NotFound("Restaurant not found");

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

        [Authorize(Roles = "Admin,Customer")]
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

        [Authorize(Roles = "Admin,Restaurant")]
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

        [Authorize(Roles = "Admin,Restaurant")]
        [HttpGet("ViewRestaurantPastOrders/{RestId}")]
        public async Task<IActionResult> ViewRestaurantPastOrders(int RestId)
        {
            var orders = await _context.Orders
                .Where(o => o.RestId == RestId && (
                    o.Status == "Delivered"
                    || o.Status == "Prepared"
                    || o.Status == "CancelledByRest"
                    || o.Status == "CancelledByCustomer"
                    || o.Status == "CancelledByDeliverer"))
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
                    o.RestId,
                    FinalPrice = o.TotalAmount - o.Discount + o.HandlingFee,
                    Items = o.OrderDetails.Select(od => new
                    {
                        od.Food.Name,
                        od.Price,
                        od.Quantity,
                        od.Food.FoodImageUrl
                    }).ToList()
                })
                .ToListAsync();

            return Ok(orders);
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

        [Authorize(Roles = "Admin,Customer")]
        [HttpGet("Search")]
        public async Task<IActionResult> SearchRestaurants([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Search term is required.");

            var matchingRestaurants = await _context.Restaurants
                .Where(r => r.Name.Contains(name) && r.RestStatus != "Deleted")  // Exclude restaurants marked as "Deleted"
                .Select(r => new RestaurantSummaryDto
                {
                    RestId = r.RestId,
                    Name = r.Name,
                    Rating = r.Rating,
                    RestImageUrl = r.RestImageUrl,
                    address=r.Address,
                    
                })
                .ToListAsync();

            return Ok(matchingRestaurants);
        }


        [Authorize(Roles = "Admin,Restaurant")]
        [HttpPut("Prepared/{orderId}")]
        public async Task<IActionResult> MarkOrderPrepared(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return NotFound("Order not found");

            order.Status = "Prepared";
            await _context.SaveChangesAsync();
            return Ok("Order marked as prepared");
        }

        [Authorize(Roles = "Admin,Restaurant")]
        [HttpPatch("SetRestaurantStatus/{RestId}")]
        public async Task<IActionResult> SetRestaurantStatus(int RestId, [FromBody] UpdateRestaurantStatusDto statusDto)
        {
            var restaurant = await _context.Restaurants.FindAsync(RestId);
            if (restaurant == null) return NotFound("Restaurant not found.");

            // Validate the status before updating
            if (statusDto.RestStatus != "Open" && statusDto.RestStatus != "Closed")
                return BadRequest("Invalid status. Use 'Open' or 'Closed'.");

            restaurant.RestStatus = statusDto.RestStatus;
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Restaurant is now {restaurant.RestStatus}.", restaurant.RestStatus });
        }



        [Authorize(Roles = "Admin,Restaurant")]
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

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
