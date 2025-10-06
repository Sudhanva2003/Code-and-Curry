using Code_Curry.DTOs;
using Code_Curry.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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

       
        [HttpPost("RegisterRestaurant")]
        public async Task<IActionResult> RegisterRestaurant([FromBody] RestaurantDto dto)
        {
            bool emailExists = await _context.Restaurants.AnyAsync(u => u.Email == dto.Email);
            bool userEmailExists = await _context.Users.AnyAsync(u => u.Email == dto.Email);
            if (emailExists || userEmailExists)
                return Conflict("Email already exists."); // 409 Conflict

            var hashedPassword = HashPassword(dto.Password);

            var restaurant = new Restaurant
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address,
                PasswordHash = hashedPassword,
                Rating = dto.Rating,
                IsOpen = dto.IsOpen,
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
                restaurant.Email,
                restaurant.Address,
                restaurant.Rating,
                restaurant.IsOpen,
                restaurant.RestImageUrl
            });
        }

       
        [HttpPut("EditRestaurant/{RestId}")]
        public async Task<IActionResult> EditRestaurant(int RestId, [FromBody] RestaurantEditDto dto)
        {
            var restaurant = await _context.Restaurants.FindAsync(RestId);
            if (restaurant == null) return NotFound("Restaurant not found");

            restaurant.Name = dto.Name;
            restaurant.Address = dto.Address;
            restaurant.Phone = dto.Phone;
            restaurant.IsOpen = dto.IsOpen;

            // Optional update for image URL
            if (!string.IsNullOrWhiteSpace(dto.RestImageUrl))
                restaurant.RestImageUrl = dto.RestImageUrl;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Restaurant updated successfully", restaurant.RestImageUrl });
        }

       
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
                IsOpen = restaurant.IsOpen,
                RestImageUrl = restaurant.RestImageUrl
            };

            return Ok(restaurantDetails);
        }

       
        [HttpGet("Menu/{RestId}")]
        public async Task<IActionResult> ViewMenu(int RestId)
        {
            var restaurant = await _context.Restaurants
                .Include(r => r.Foods)
                .FirstOrDefaultAsync(r => r.RestId == RestId);

            if (restaurant == null) return NotFound("Restaurant not found");

            var menu = restaurant.Foods.Select(f => new MenuItemDto
            {
                FoodId = f.FoodId,
                Name = f.Name,
                Category = f.Category,
                Description = f.Description,
                Price = f.Price,
                IsAvailable = f.IsAvailable,
                FoodImageUrl = f.FoodImageUrl
            }).ToList();

            return Ok(menu);
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

       
        [HttpGet("Home")]
        public async Task<IActionResult> GetRestaurantsByRating()
        {
            var restaurants = await _context.Restaurants
                .Where(r => r.IsOpen)
                .OrderByDescending(r => r.Rating)
                .Select(r => new RestaurantSummaryDto
                {
                    RestId = r.RestId,
                    Name = r.Name,
                    Rating = r.Rating,
                    IsOpen = r.IsOpen,
                    RestImageUrl = r.RestImageUrl
                }).ToListAsync();

            return Ok(restaurants);
        }

    
        [HttpGet("ViewRestaurantOpenOrders/{RestId}")]
        public async Task<IActionResult> ViewRestaurantOpenOrders(int RestId)
        {
            var orders = await _context.Orders
                .Where(o => o.RestId == RestId && o.Status == "Paid")
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
                .Where(o => o.RestId == RestId && o.Status == "Prepared")
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

       
        [HttpDelete("DeleteRestaurant/{RestId}")]
        public async Task<IActionResult> DeleteRestaurant(int RestId)
        {
            var restaurant = await _context.Restaurants.FindAsync(RestId);
            if (restaurant == null)
                return NotFound(new { message = "Restaurant not found" });

            _context.Restaurants.Remove(restaurant);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Restaurant deleted successfully" });
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
