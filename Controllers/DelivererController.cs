using Code_Curry.DTOs;
using Code_Curry.Models;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Code_Curry.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DelivererController : ControllerBase
    {
        private readonly CodeCurryContext _context;

        public DelivererController(CodeCurryContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        [HttpPost("DelivererRegister")]
        public async Task<IActionResult> DelivererRegister([FromBody] DelivererDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (existingUser != null)
                return Conflict(new { message = "Email already registered." });

            var passwordHash = HashPassword(request.Password);

            var deliverer = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                Address = request.Address,
                PasswordHash = passwordHash,
                Role = "Deliverer",
                LicenseNumber = request.LicenseNumber,
                VehicleNumber = request.VehicleNumber,
                UserStatus = "Active"
            };

            _context.Users.Add(deliverer);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Deliverer registered successfully", delivererId = deliverer.UserId });
        }
        [Authorize(Roles = "Admin,Deliverer")]
        [HttpGet("ViewLiveOrders")]
        public async Task<IActionResult> ViewLiveOrders()
        {
            var liveOrders = await _context.Orders
                .Include(o => o.Rest)
                .Include(o => o.User)
                .Where(o => (o.Status == "Prepared" || o.Status == "Paid")
                            && o.Rest.RestStatus != "Deleted"
                            && o.User.UserStatus != "Deleted")
                .Select(o => new
                {
                    o.OrderId,
                    o.Status,
                    RestaurantAddress = o.Rest.Address,
                    CustomerAddress = o.User.Address,
                    o.TotalAmount
                })
                .ToListAsync();

            return Ok(liveOrders);
        }

        [Authorize(Roles = "Admin,Deliverer")]
        [HttpPut("AssignOrder/{orderId}")]
        public async Task<IActionResult> AssignOrder(int orderId, [FromBody] AssignOrderRequest request)
        {
            var delivererId = request.DelivererId;

            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                return NotFound("Order not found.");

            var deliverer = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == delivererId && u.Role == "Deliverer" && u.UserStatus != "Deleted");

            if (deliverer == null)
                return BadRequest("Invalid or deleted deliverer.");

            order.DelivererId = delivererId;
            order.Status = "Assigned";

            await _context.SaveChangesAsync();
            return Ok("Order assigned successfully.");
        }

        [Authorize(Roles = "Admin,Deliverer")]
        [HttpGet("DeliveryDetail/{orderId}")]
        public async Task<IActionResult> DeliveryDetail(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Food)
                .Include(o => o.Rest)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
                return NotFound("Order not found.");

            var result = new
            {
                RestaurantName = order.Rest?.Name,
                RestaurantAddress = order.Rest?.Address,
                CustomerName = order.User?.FullName,
                CustomerAddress = order.User?.Address,
                Items = order.OrderDetails.Select(od => new
                {
                    od.Food.Name,
                    od.Quantity
                }).ToList()
            };

            return Ok(result);
        }

        [Authorize(Roles = "Admin,Deliverer")]
        [HttpPut("MarkDelivered/{orderId}")]
        public async Task<IActionResult> MarkDelivered(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                return NotFound("Order not found.");

            order.Status = "Delivered";
            order.OrderDate = DateTime.UtcNow; // ✅ Set delivery date

            await _context.SaveChangesAsync();
            return Ok("Order marked as delivered.");
        }

        [HttpGet("ViewDeliveredOrders/{delivererId}")]
        public async Task<IActionResult> ViewDeliveredOrders(int delivererId)
        {
            var orders = await _context.Orders
                .Include(o => o.Rest)
                .Include(o => o.User)
                .Where(o => o.DelivererId == delivererId && o.Status == "Delivered")
                .Select(o => new
                {
                    o.OrderId,
                    RestaurantAddress = o.Rest.Address,
                    CustomerAddress = o.User.Address,
                    o.DeliveryFee,
                    o.OrderDate // ✅ Include delivery date for filtering
                })
                .ToListAsync();

            return Ok(orders);
        }

        [Authorize(Roles = "Admin,Deliverer")]
        [HttpGet("DelivererProfile/{delivererId}")]
        public async Task<IActionResult> DelivererProfile(int delivererId)
        {
            var deliverer = await _context.Users
                .Where(u => u.UserId == delivererId && u.Role == "Deliverer" && u.UserStatus != "Deleted")
                .Select(u => new DelivererProfileDto
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    Phone = u.Phone,
                    Address = u.Address,
                    Rating = u.Rating,
                    VehicleNumber = u.VehicleNumber!,
                    LicenseNumber = u.LicenseNumber!
                })
                .FirstOrDefaultAsync();

            if (deliverer == null)
                return NotFound("Deliverer not found.");

            return Ok(deliverer);
        }

        [Authorize(Roles = "Admin,Deliverer")]
        [HttpPut("EditDeliverer/{delivererId}")]
        public async Task<IActionResult> EditDeliverer(int delivererId, [FromBody] EditDelivererDto dto)
        {
            var deliverer = await _context.Users.FirstOrDefaultAsync(u => u.UserId == delivererId && u.Role == "Deliverer");
            if (deliverer == null)
                return NotFound("Deliverer not found.");

            deliverer.FullName = dto.FullName;
            deliverer.Phone = dto.Phone;
            deliverer.Address = dto.Address;
            deliverer.VehicleNumber = dto.VehicleNumber;

            await _context.SaveChangesAsync();
            return Ok("Deliverer details updated successfully.");
        }

        [Authorize(Roles = "Admin,Deliverer")]
        [HttpDelete("DeleteDeliverer/{delivererId}")]
        public async Task<IActionResult> DeleteDeliverer(int delivererId)
        {
            var deliverer = await _context.Users.FirstOrDefaultAsync(u => u.UserId == delivererId && u.Role == "Deliverer");
            if (deliverer == null)
                return NotFound("Deliverer not found.");

            deliverer.UserStatus = "Deleted";
            await _context.SaveChangesAsync();

            return Ok("Deliverer soft-deleted successfully.");
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
