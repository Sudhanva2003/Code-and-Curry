using Code_Curry.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore; // required for AnyAsync
using Code_Curry.DTOs;

namespace Code_Curry.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly CodeCurryContext _context;

        public UsersController(CodeCurryContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CreateUserDto dto)
        {
            // async check if email exists
            bool emailExists = await _context.Users.AnyAsync(u => u.Email == dto.Email);
            if (emailExists)
            {
                return Conflict("Email already exists."); // 409 Conflict
            }

            // hash password
            var hashedPassword = HashPassword(dto.Password); //hashpassword is defined below,
                                                             //we have implemented this function.

            // map DTO to EF entity
            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address,
                PasswordHash = hashedPassword,
                Role = dto.Role ?? "Customer" // take role from input, default to Customer
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

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
