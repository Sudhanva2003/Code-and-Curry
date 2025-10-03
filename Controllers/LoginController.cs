using Code_Curry.Dtos;
using Code_Curry.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Code_Curry.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly CodeCurryContext _context;

        public LoginController(CodeCurryContext context)
        {
            _context = context;
        }

        // --------------------------
        // POST api/login
        // Login for Customers & Restaurants
        // --------------------------
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Email and Password are required.");

            var email = dto.Email.Trim().ToLower();

            // -----------------------------------------
            // Step 1: Check Restaurant table first
            // (only to see if email exists as a Restaurant contact)
            // -----------------------------------------
            var restaurant = await _context.Restaurants
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Email.ToLower() == email);

            if (restaurant != null)
            {
                // Found a Restaurant with this email → now validate against Users
                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == email && u.Role == "Restaurant");

                if (user == null || user.PasswordHash != dto.Password) // plaintext compare for now
                    return Unauthorized("Invalid credentials");

                // ✅ Restaurant login → return only RestId
                return Ok(new
                {
                    Source = "Restaurant",
                    Email = user.Email,
                    Role = "Restaurant",
                    RestId = restaurant.RestId
                });
            }

            // -----------------------------------------
            // Step 2: If not a Restaurant, check Users table (Customers)
            // -----------------------------------------
            var normalUser = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email);

            if (normalUser != null)
            {
                if (normalUser.PasswordHash != dto.Password) // plaintext compare for now
                    return Unauthorized("Invalid credentials");

                if (normalUser.Role != "Customer")
                    return Unauthorized("Only Customer login allowed here");

                // ✅ Customer login → return UserId
                return Ok(new
                {
                    Source = "Users",
                    Email = normalUser.Email,
                    Role = "Customer",
                    UserId = normalUser.UserId
                });
            }

            // -----------------------------------------
            // Step 3: If email not found in either table
            // -----------------------------------------
            return Unauthorized("Invalid credentials");
        }
    }
}
