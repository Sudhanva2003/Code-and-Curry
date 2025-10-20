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
    }
}
