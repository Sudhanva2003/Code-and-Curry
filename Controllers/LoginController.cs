using Code_Curry.DTOs;
using Code_Curry.Models;
using Code_Curry.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Code_Curry.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly CodeCurryContext _context;
        private readonly JwtService _jwtService;

        public LoginController(CodeCurryContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Password))
                return BadRequest("Email and password are required.");

            // Find matching users in all roles
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email && u.Role == "Customer" && u.UserStatus != "Deleted");
            var restaurant = await _context.Restaurants.FirstOrDefaultAsync(r => r.Email == dto.Email && r.RestStatus != "Deleted");
            var deliverer = await _context.Users.FirstOrDefaultAsync(d => d.Email == dto.Email && d.Role == "Deliverer" && d.UserStatus != "Deleted");
            var admin = await _context.Users.FirstOrDefaultAsync(a => a.Email == dto.Email && a.Role == "Admin" && a.UserStatus != "Deleted");

            if (user == null && restaurant == null && deliverer == null && admin == null)
                return Unauthorized("Email not found.");

            string hashedPassword = HashPassword(dto.Password);

            if (user != null)
            {
                if (user.PasswordHash != hashedPassword)
                    return Unauthorized("Invalid password.");

                var token = _jwtService.GenerateToken(user.UserId.ToString(), user.Email, "Customer");
                return Ok(new LoginResponseDto
                {
                    Email = dto.Email,
                    UserId = user.UserId,
                    Role = "Customer",
                    Name = user.FullName,
                    Token = token
                });
            }

            if (restaurant != null)
            {
                if (restaurant.PasswordHash != hashedPassword)
                    return Unauthorized("Invalid password.");

                var token = _jwtService.GenerateToken(restaurant.RestId.ToString(), restaurant.Email, "Restaurant");
                return Ok(new LoginResponseDto
                {
                    Email = dto.Email,
                    UserId = restaurant.RestId,
                    Role = "Restaurant",
                    Name = restaurant.Name,
                    Token = token
                });
            }

            if (deliverer != null)
            {
                if (deliverer.PasswordHash != hashedPassword)
                    return Unauthorized("Invalid password.");

                var token = _jwtService.GenerateToken(deliverer.UserId.ToString(), deliverer.Email, "Deliverer");
                return Ok(new LoginResponseDto
                {
                    Email = dto.Email,
                    UserId = deliverer.UserId,
                    Role = "Deliverer",
                    Name = deliverer.FullName,
                    Token = token
                });
            }

            if (admin != null)
            {
                if (admin.PasswordHash != hashedPassword)
                    return Unauthorized("Invalid password.");

                var token = _jwtService.GenerateToken(admin.UserId.ToString(), admin.Email, "Admin");
                return Ok(new LoginResponseDto
                {
                    Email = dto.Email,
                    UserId = admin.UserId,
                    Role = "Admin",
                    Name = admin.FullName,
                    Token = token
                });
            }

            return Unauthorized("Login failed.");
        }

        private string HashPassword(string password)
        {
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
