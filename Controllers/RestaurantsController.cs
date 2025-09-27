using Code_Curry.DTOs;
using Code_Curry.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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

        [HttpPost("AddRestaurant")]
        public async Task<IActionResult> AddRestaurant([FromBody] RestaurantDto dto)
        {
            var restaurant = new Restaurant
            {
                Name = dto.Name,
                Address = dto.Address,
                Rating = dto.Rating,
                Phone = dto.Phone,
                Email = dto.Email,
                IsOpen = dto.IsOpen
            };

            _context.Restaurants.Add(restaurant);
            await _context.SaveChangesAsync();
            return Ok(restaurant.RestId);
        }
        [HttpPut("EditRestaurant")]
        public async Task<IActionResult> EditRestaurant(int id, [FromBody] RestaurantUpdateDto dto)
        {
            var restaurant = await _context.Restaurants.FindAsync(id);
            if (restaurant == null) return NotFound("Restaurant not found");

            restaurant.Name = dto.Name;
            restaurant.Address = dto.Address;
            restaurant.Rating = dto.Rating;
            restaurant.Phone = dto.Phone;
            restaurant.Email = dto.Email;
            restaurant.IsOpen = dto.IsOpen;

            await _context.SaveChangesAsync();
            return Ok("Restaurant updated");
        }

        [HttpGet("Menu")]
        public async Task<IActionResult> ViewMenu(int id)
        {
            var restaurant = await _context.Restaurants
                .Include(r => r.Foods)
                .FirstOrDefaultAsync(r => r.RestId == id);

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

        [HttpPut("OrderDetails")]
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
    }
}
