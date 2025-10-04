using Code_Curry.DTOs;
using Code_Curry.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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
            bool userEmailExists= await _context.Users.AnyAsync(u => u.Email == dto.Email);
            if (emailExists||userEmailExists)
            {
                return Conflict("Email already exists."); // 409 Conflict
            }

            // hash password
            var hashedPassword = HashPassword(dto.Password); //hashpassword is defined below,
                                                             //we have implemented this function.

            // map DTO to EF entity
            var Restaurant = new Restaurant
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address,
                PasswordHash = hashedPassword,
                Rating = dto.Rating,
                IsOpen = dto.IsOpen
                //Role = dto.Role ?? "Customer" // take role from input, default to Customer
            };

            await _context.Restaurants.AddAsync(Restaurant);           // async add
            await _context.SaveChangesAsync();             // async save

            // return minimal info, do not return password
            return Ok(new
            {
                Restaurant.RestId,
                Restaurant.Name,
                Restaurant.Email,
                Restaurant.Address,
                Restaurant.Rating,
                Restaurant.IsOpen

            });
        }
        [HttpPut("EditRestaurant/{RestId}")]
        public async Task<IActionResult> EditRestaurant(int RestId, [FromBody] RestaurantEditDto dto)
        {
            var restaurant = await _context.Restaurants.FindAsync(RestId);
            if (restaurant == null) return NotFound("Restaurant not found");

            var restaurantDetails = new RestaurantEditDto
            {
                Name = restaurant.Name,
                Address = restaurant.Address,
                Phone = restaurant.Phone,
                IsOpen = restaurant.IsOpen

            };

            return Ok(restaurantDetails);
        }
        [HttpGet("ViewRestaurant/{RestId}")]
        public async Task<IActionResult> ViewRestaurant(int RestId)
        {
            var restaurant = await _context.Restaurants
                .FirstOrDefaultAsync(r => r.RestId == RestId);

            if (restaurant == null) return NotFound("Restaurant not found");

            var restaurantDetails = new RestaurantProfileDto
            {
                Name = restaurant.Name,
                Address = restaurant.Address,
                Rating =restaurant.Rating,
                Phone = restaurant.Phone,
                Email = restaurant.Email,
                IsOpen = restaurant.IsOpen

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
                IsAvailable = f.IsAvailable
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
                    IsOpen = r.IsOpen
                }).ToListAsync();

            return Ok(restaurants);
        }


        

        // 1) View Open Orders
        [HttpGet("ViewRestaurantOpenOrders/{RestId}")]
        public async Task<IActionResult> ViewRestaurantOpenOrders(int RestId)
        {
            var orders = await _context.Orders
                .Where(o => o.RestId == RestId
                            && o.Status == "Paid")
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
                        od.Quantity
                    }).ToList()
                })
                .ToListAsync();

            return Ok(orders);
        }

        // 2) View Past Orders
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
                        od.Quantity
                    }).ToList()
                })
                .ToListAsync();

            return Ok(orders);
        }


        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
