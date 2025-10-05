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

            // Fetch all foods for price and restaurant info
            var foodIds = dto.OrderItems.Select(x => x.FoodId).ToList();
            var foods = await _context.Foods
                .Where(f => foodIds.Contains(f.FoodId))
                .ToListAsync();

            if (foods.Count != foodIds.Count)
                return BadRequest("Some food items not found.");

            // Group order items by restaurant
            var itemsByRest = dto.OrderItems
                .Join(foods, i => i.FoodId, f => f.FoodId, (i, f) => new { Item = i, Food = f })
                .GroupBy(x => x.Food.RestId);

            var generatedBills = new List<object>(); // anonymous object for OrderId on top

            foreach (var group in itemsByRest)
            {
                var order = new Order
                {
                    UserId = dto.UserId,
                    RestId = group.Key,
                    OrderDate = DateTime.UtcNow,
                    Status = "Paid",
                };

                var orderDetails = new List<OrderDetail>();

                foreach (var x in group)
                {
                    var detail = new OrderDetail
                    {
                        FoodId = x.Food.FoodId,
                        Quantity = x.Item.Quantity,
                        Price = 0 // will be calculated in Bill
                    };

                    orderDetails.Add(detail);
                    order.OrderDetails.Add(detail);
                }

                // Generate the bill totals
                var bill = Bill.GenerateBill(orderDetails, foods.Where(f => f.RestId == group.Key).ToList());

                // Set total amount in order
                order.TotalAmount = bill.FinalAmount;

                // Add order to context
                _context.Orders.Add(order);
                await _context.SaveChangesAsync(); // OrderId populated here

                // Add OrderId on top of bill response
                generatedBills.Add(new
                {
                    orderId = order.OrderId,
                    items = bill.Items,
                    subtotal = bill.Subtotal,
                    sgst = bill.SGST,
                    cgst = bill.CGST,
                    handlingCharges = bill.HandlingCharges,
                    deliveryFees = bill.DeliveryFees,
                    finalAmount = bill.FinalAmount
                });
            }

            return Ok(generatedBills);
        }
    }
}
