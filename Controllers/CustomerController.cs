using Code_Curry.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore; // required for AnyAsync
using Code_Curry.DTOs;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

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

        [HttpGet("ViewUserOrders/{UserId}")]
        public async Task<IActionResult> ViewOrders(int UserId)
        {
            var orders = await _context.Orders
                .Where(o => o.UserId == UserId)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Food)
                .ToListAsync();

            if (!orders.Any())
                return NotFound("No orders found for this user.");

            var openOrders = orders
    .Where(o => o.Status == "Paid" || o.Status == "Prepared" 
    || o.Status=="Assigned")
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

            var pastOrders = orders
      .Where(o => o.Status == "Delivered"
         || o.Status == "Prepared"
         || o.Status == "CancelledByCustomer"
         || o.Status == "CancelledByRest"
         || o.Status == "CancelledByDeliverer")
  .OrderByDescending(o => o.OrderDate)
.Select(o => new
{
    orderId = o.OrderId,
    delivererId = o.DelivererId,
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


            return Ok(new { openOrders, pastOrders });
        }



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
        [HttpPut("CancelOrder/{orderId}")]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return NotFound();

            // Only allow cancellation if order is still active
            if (order.Status == "Paid" || order.Status == "Prepared" || order.Status=="Assigned")
            {
                order.Status = "CancelledByCustomer";
                await _context.SaveChangesAsync();
                return Ok("Order cancelled by customer.");
            }

            return BadRequest("Order cannot be cancelled in its current state.");
        }





    }
}
