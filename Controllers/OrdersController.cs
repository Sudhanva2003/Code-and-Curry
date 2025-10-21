using Code_Curry.DTOs;
using Code_Curry.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Code_Curry.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly CodeCurryContext _context;

        public OrdersController(CodeCurryContext context)
        {
            _context = context;
        }

        [HttpPost("placeOrder")]
        public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderDto dto)
        {
            if (dto == null || dto.OrderItems == null || !dto.OrderItems.Any())
                return BadRequest("Order must have at least one item.");

            var foodIds = dto.OrderItems.Select(x => x.FoodId).ToList();
            var foods = await _context.Foods
                .Where(f => foodIds.Contains(f.FoodId) && f.FoodStatus == "Available")
                .ToListAsync();

            if (foods.Count != foodIds.Count)
                return BadRequest("Some food items not found or unavailable.");

            var itemsByRest = dto.OrderItems
                .Join(foods, i => i.FoodId, f => f.FoodId, (i, f) => new { Item = i, Food = f })
                .GroupBy(x => x.Food.RestId);

            var generatedBills = new List<object>();

            foreach (var group in itemsByRest)
            {
                var order = new Order
                {
                    UserId = dto.UserId,
                    RestId = group.Key,
                    OrderDate = DateTime.UtcNow,
                    Status = "Paid"
                };

                var orderDetails = new List<OrderDetail>();

                foreach (var x in group)
                {
                    var detail = new OrderDetail
                    {
                        FoodId = x.Food.FoodId,
                        Quantity = x.Item.Quantity,
                        Price = x.Food.Price * x.Item.Quantity
                    };

                    orderDetails.Add(detail);
                    order.OrderDetails.Add(detail);
                }

                // Generate the bill
                var bill = Bill.GenerateBill(orderDetails, foods.Where(f => f.RestId == group.Key).ToList());

                // Populate new fields in order
                order.TotalAmount = bill.Subtotal;
                order.Discount = bill.Discount;
                order.HandlingFee = bill.HandlingCharges;
                order.PlatformFee = bill.PlatformFee;
                order.DeliveryFee = bill.DeliveryFees;
                order.GST = bill.SGST + bill.CGST;
                order.FinalPrice = bill.FinalAmount;

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                generatedBills.Add(new
                {
                    orderId = order.OrderId,
                    items = bill.Items, 
                    subtotal = bill.Subtotal,
                    discount = bill.Discount,
                    gst = bill.SGST + bill.CGST,
                    handlingFee = bill.HandlingCharges,
                    platformFee = bill.PlatformFee,
                    deliveryFee = bill.DeliveryFees,
                    finalPrice = bill.FinalAmount
                });
            }

            return Ok(generatedBills);
        }
        // GET: api/Orders/RestaurantOrders?restId=1&page=1&pageSize=10&month=10
        [HttpGet("RestaurantOrders")]
        public async Task<IActionResult> GetRestaurantOrders(
            [FromQuery] int restId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? month = null)
        {
            if (restId <= 0)
                return BadRequest("Restaurant ID is required.");

            var query = _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Food)
                .Where(o => o.RestId == restId);

            if (month.HasValue)
            {
                query = query.Where(o => o.OrderDate.Month == month.Value);
            }

            var totalOrders = await query.CountAsync();

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new
                {
                    o.OrderId,
                    o.UserId,
                    o.RestId,
                    o.OrderDate,
                    o.TotalAmount,
                    Items = o.OrderDetails.Select(od => new
                    {
                        od.FoodId,
                        od.Quantity,
                        od.Price,
                        FoodName = od.Food.Name
                    })
                })
                .ToListAsync();

            return Ok(new
            {
                totalOrders,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(totalOrders / (double)pageSize),
                orders
            });
        }
        // GET: api/Orders/CustomerOrders?customerId=1&page=1&pageSize=10&month=10
        [HttpGet("CustomerOrders")]
        public async Task<IActionResult> GetCustomerOrders(
            [FromQuery] int customerId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? month = null)
        {
            if (customerId <= 0)
                return BadRequest("Customer ID is required.");

            var query = _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Food)
                .Where(o => o.UserId == customerId);

            if (month.HasValue)
            {
                query = query.Where(o => o.OrderDate.Month == month.Value);
            }

            var totalOrders = await query.CountAsync();

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new
                {
                    o.OrderId,
                    o.UserId,
                    o.RestId,
                    o.OrderDate,
                    o.TotalAmount,
                    Items = o.OrderDetails.Select(od => new
                    {
                        od.FoodId,
                        od.Quantity,
                        od.Price,
                        FoodName = od.Food.Name
                    })
                })
                .ToListAsync();

            return Ok(new
            {
                totalOrders,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(totalOrders / (double)pageSize),
                orders
            });
        }


    }
}
