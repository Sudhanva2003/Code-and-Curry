using Code_Curry.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Code_Curry.DTOs;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Code_Curry.Services; // Add reference to DistanceService

namespace Code_Curry.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly CodeCurryContext _context;
        private readonly DistanceService _distanceService; // Add reference to DistanceService

        public CustomerController(CodeCurryContext context, DistanceService distanceService)
        {
            _context = context;
            _distanceService = distanceService;  // Initialize DistanceService
        }

        // Existing methods here ...

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CustomerCreateDto dto)
        {
            // async check if email exists
            bool emailExists = await _context.Users.AnyAsync(u => u.Email == dto.Email);
            bool RestaurantEmailExists = await _context.Restaurants.AnyAsync(u => u.Email == dto.Email);
            if (emailExists || RestaurantEmailExists)
            {
                return Conflict("Email already exists."); // 409 Conflict
            }

            // hash password
            var hashedPassword = HashPassword(dto.Password); // hashpassword is defined below.

            // map DTO to EF entity
            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address,
                PasswordHash = hashedPassword,
                Role = "Customer"
            };

            await _context.Users.AddAsync(user);           // async add
            await _context.SaveChangesAsync();             // async save

            // return minimal info, do not return password
            return Ok(new
            {
                user.UserId,
                user.FullName,
                user.Email,
                user.Role
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Password))
                return BadRequest("Email and password are required.");

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

                return Ok(new LoginResponseDto
                {
                    Email = dto.Email,
                    UserId = user.UserId,
                    Role = "customer",
                    Name = user.FullName
                });
            }

            if (restaurant != null)
            {
                if (restaurant.PasswordHash != hashedPassword)
                    return Unauthorized("Invalid password.");

                return Ok(new LoginResponseDto
                {
                    Email = dto.Email,
                    UserId = restaurant.RestId,
                    Role = "restaurant",
                    Name = restaurant.Name
                });
            }

            if (deliverer != null)
            {
                if (deliverer.PasswordHash != hashedPassword)
                    return Unauthorized("Invalid password.");

                return Ok(new LoginResponseDto
                {
                    Email = dto.Email,
                    UserId = deliverer.UserId,
                    Role = "deliverer",
                    Name = deliverer.FullName
                });
            }

            if (admin != null)
            {
                if (admin.PasswordHash != hashedPassword)
                    return Unauthorized("Invalid password.");

                return Ok(new LoginResponseDto
                {
                    Email = dto.Email,
                    UserId = admin.UserId,
                    Role = "admin",
                    Name = admin.FullName
                });
            }

            return Unauthorized("Login failed.");
        }

        [HttpGet("ViewUser/{UserId}")]
        public async Task<IActionResult> ViewUser(int UserId)
        {
            var user = await _context.Users.FindAsync(UserId);
            if (user == null)
            {
                return NotFound();
            }
            var dto = new CustomerSummaryDto
            {
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                Role = user.Role
            };

            return Ok(dto);
        }

        [HttpPut("EditUserDetails/{UserId}")]
        public async Task<IActionResult> EditUserDetails(int UserId, [FromBody] CustomerEditDto newUser)
        {
            var oldUser = await _context.Users.FindAsync(UserId);
            if (oldUser == null)
            {
                return BadRequest("User not found");
            }
            if (newUser == null)
            {
                return BadRequest("Updated User Details not given");
            }
            oldUser.FullName = newUser.FullName;
            oldUser.Phone = newUser.Phone;
            oldUser.Address = newUser.Address;

            await _context.SaveChangesAsync();

            var dto = new CustomerEditDto
            {
                FullName = oldUser.FullName,
                Phone = oldUser.Phone,
                Address = oldUser.Address,
            };

            return Ok(dto);
        }

        // New Distance Method to calculate the distance
        [HttpGet("Distance")]
        public async Task<int> Distance(string restAddress, string customerAddress)
        {
            // Call the DistanceService to calculate the distance
            int distance = await _distanceService.GetDistanceAsync(restAddress, customerAddress);
            return distance;  // Return the distance as an integer
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
