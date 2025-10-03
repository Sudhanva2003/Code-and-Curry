using Code_Curry.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore; // required for AnyAsync
using Code_Curry.DTOs;
using Microsoft.AspNetCore.Cors;

namespace Code_Curry.Controllers
{
   
    [Route("api/[controller]")]
    [ApiController]
    
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

        [HttpPut("EditUserDetails/{userId}")]
        public async Task<IActionResult> EditUserDetails(int id,[FromBody] CreateUserDto newUser)
        {
            var oldUser = await _context.Users.FindAsync(id);
            if (oldUser==null)
            {
                return BadRequest("User not found");
            }
            if (newUser == null)
            {
                return BadRequest("Updated User Details not given");
            }
            oldUser.FullName = newUser.FullName;
            oldUser.Email = newUser.Email;
            oldUser.Phone = newUser.Phone;
            oldUser.Address = newUser.Address;

            await _context.SaveChangesAsync();
            return Ok(newUser);

        }

        [HttpGet("ViewUserOrders/{userId}")]
        public async Task<IActionResult> ViewOrders(int userId)
        {
            
                // Fetch all orders for this user including details and food
                var orders = await _context.Orders
                    .Where(o => o.UserId == userId)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Food)
                    .ToListAsync();

                if (!orders.Any())
                    return NotFound("No orders found for this user.");

                // Open Orders (status = Paid)
                var openOrders = orders
                    .Where(o => o.Status == "Paid")
                    .Select(o => new
                    {
                        orderId = o.OrderId,
                        restId = o.RestId,
                        orderDate = o.OrderDate,
                        totalAmount = o.TotalAmount,
                        status = o.Status,
                        items = o.OrderDetails.Select(d => new
                        {
                            foodId = d.FoodId,
                            foodName = d.Food.Name,
                            quantity = d.Quantity,
                            price = d.Price
                        })
                    }).ToList();

                // Past Orders (status = Delivered)
                var pastOrders = orders
                    .Where(o => o.Status == "Delivered")
                    .Select(o => new
                    {
                        orderId = o.OrderId,
                        restId = o.RestId,
                        orderDate = o.OrderDate,
                        totalAmount = o.TotalAmount,
                        status = o.Status,
                        items = o.OrderDetails.Select(d => new
                        {
                            foodId = d.FoodId,
                            foodName = d.Food.Name,
                            quantity = d.Quantity,
                            price = d.Price
                        })
                    }).ToList();

                var result = new
                {
                    openOrders,
                    pastOrders
                };

                return Ok(result);
            


        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
