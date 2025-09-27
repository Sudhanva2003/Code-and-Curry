using Code_Curry.DTOs;
using Code_Curry.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Code_Curry.Controllers
{
    
        [ApiController]
        [Route("api/[controller]")]

        public class OrdersController : ControllerBase
        {
            private readonly CodeCurryContext _context;
            
            public OrdersController(CodeCurryContext context)
            {
            this._context = context;
            }

        [HttpPost]
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

            var generatedBills = new List<BillDto>();
            var billService = new Bill(); // instance of your Bill class

            foreach (var group in itemsByRest)
            {
                var order = new Order
                {
                    UserId = dto.UserId,
                    RestId = group.Key,
                    OrderDate = DateTime.UtcNow,
                    Status = "Pending",
                };

                var orderDetails = new List<OrderDetail>();

                // Create OrderDetail objects (Price left 0, bill will calculate)
                foreach (var x in group)
                {
                    var detail = new OrderDetail
                    {
                        FoodId = x.Food.FoodId,
                        Quantity = x.Item.Quantity,
                        Price = 0 // Price will be calculated in GenerateBill
                    };

                    orderDetails.Add(detail);
                    order.OrderDetails.Add(detail);
                }

                // Call GenerateBill to get final amount including taxes, delivery, handling
                var bill = Bill.GenerateBill(orderDetails, foods.Where(f => f.RestId == group.Key).ToList());

                // Set the TotalAmount in Order to final bill amount
                order.TotalAmount = bill.FinalAmount;

                // Save order to context
                _context.Orders.Add(order);

                // Add the generated bill to response
                generatedBills.Add(bill);
            }

            await _context.SaveChangesAsync();

            // Return the full generated bills for all restaurants
            return Ok(generatedBills);
        }







    }

}
