using Code_Curry.DTOs;
using Code_Curry.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // required for AnyAsync
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Security.Cryptography;
using System.Text;

namespace Code_Curry.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly CodeCurryContext _context;

        public CustomerController(CodeCurryContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CustomerCreateDto dto)
        {
            // async check if email exists
            bool emailExists = await _context.Users.AnyAsync(u => u.Email == dto.Email);
            bool RestaurantEmailExists = await _context.Restaurants.AnyAsync(u => u.Email == dto.Email);
            if (emailExists||RestaurantEmailExists)
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




        [Authorize(Roles = "Admin,Customer")]
        [HttpGet("ViewUser/{UserId}")]

        public async Task<IActionResult> ViewUser(int UserId)
        {
            var user=await _context.Users.FindAsync(UserId);
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
                Role=user.Role
            };

            return Ok(dto);
        }

        [Authorize(Roles = "Admin,Customer")]
        [HttpPut("EditUserDetails/{UserId}")]
        public async Task<IActionResult> EditUserDetails(int UserId,[FromBody] CustomerEditDto newUser)
        {
            var oldUser = await _context.Users.FindAsync(UserId);
            if (oldUser==null)
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

        [Authorize(Roles = "Admin,Customer")]
        [HttpGet("ViewUserOrders/{UserId}")]
        public async Task<IActionResult> ViewOrders(int UserId)
        {
            // Fetch all orders for this user including details and food
            var orders = await _context.Orders
                .Where(o => o.UserId == UserId)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Food)
                .ToListAsync();

            if (!orders.Any())
                return NotFound("No orders found for this user.");

            // Open Orders (status = Paid)
            var openOrders = orders
                .Where(o => o.Status == "Paid")
                .OrderByDescending(o => o.OrderDate)
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

            // Past Orders (status = Prepared)
            var pastOrders = orders
                .Where(o => o.Status == "Prepared"|| o.Status=="Delivered")
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new
                {
                    orderId = o.OrderId,
                    delivererId=o.DelivererId,
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

        [Authorize(Roles = "Admin,Customer")]
        [HttpGet("ViewCart/{UserId}")]
        public async Task<IActionResult> ViewCart(int UserId)
        {
            var pendingOrders = await _context.Orders
                .Where(o => o.UserId == UserId && o.Status == "Pending")
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Food)
                .ToListAsync();

            if (!pendingOrders.Any())
                return NotFound("No pending orders (cart is empty).");

            var result = pendingOrders.Select(o => new
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
            });

            return Ok(result);
        }

        [Authorize(Roles = "Admin,Customer")]
        [HttpPost("Checkout/{orderId}")]
        public async Task<IActionResult> Checkout(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);

            if (order == null)
                return NotFound("Order not found.");

            if (order.Status != "Pending")
                return BadRequest("Only pending orders can be checked out.");

            order.Status = "Paid";
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Order checked out successfully.",
                orderId = order.OrderId,
                newStatus = order.Status,
                totalAmount = order.TotalAmount
            });
        }

        [Authorize(Roles = "Admin,Customer")]
        [HttpDelete("DeleteUser/{UserId}")]
        public async Task<IActionResult> DeleteUser(int UserId)
        {
            var user = await _context.Users.FindAsync(UserId);
            if (user == null || user.UserStatus == "Deleted")
                return NotFound(new { message = "User not found" });

            // Soft delete
            user.UserStatus = "Deleted";
            await _context.SaveChangesAsync();

            return Ok(new { message = "User deleted successfully (soft deleted)" });
        }

        [Authorize(Roles = "Admin,Customer")]
        [HttpPost("registerAdmin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] AdminCreateDto dto)
        {
            // async check if email exists
            bool emailExists = await _context.Users.AnyAsync(u => u.Email == dto.Email);
            bool RestaurantEmailExists = await _context.Restaurants.AnyAsync(u => u.Email == dto.Email);
            if (emailExists || RestaurantEmailExists)
            {
                return Conflict("Email already exists."); // 409 Conflict
            }

            // hash password
            var hashedPassword = HashPassword(dto.Password); // Assuming HashPassword is implemented to hash the password

            // map DTO to EF entity
            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Phone = dto.Phone,
                PasswordHash = hashedPassword, // Store the hashed password
                Role = "Admin"
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
