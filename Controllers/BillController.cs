using Code_Curry.DTOs;
using Code_Curry.Models;
using System.Collections.Generic;
using System.Linq;

namespace Code_Curry.Controllers
{
    public class Bill
    {
        public static BillDto GenerateBill(List<OrderDetail> orderDetails, List<Food> foods)
        {
            var bill = new BillDto();
            decimal subtotal = 0;
            int totalFoodItems = 0;

            foreach (var od in orderDetails)
            {
                var food = foods.First(f => f.FoodId == od.FoodId);
                decimal linePrice = food.Price * od.Quantity;
                od.Price = linePrice;

                bill.Items.Add(new BillItemDto
                {
                    FoodId = food.FoodId,
                    FoodName = food.Name,
                    Quantity = od.Quantity,
                    UnitPrice = food.Price,
                    LinePrice = linePrice
                });

                subtotal += linePrice;
                totalFoodItems += od.Quantity;
            }

            bill.Subtotal = subtotal;
            bill.SGST = subtotal * 0.09m;
            bill.CGST = subtotal * 0.09m;
            bill.HandlingCharges = totalFoodItems * 5;
            bill.DeliveryFees = 50;
            bill.FinalAmount = subtotal + bill.SGST + bill.CGST + bill.HandlingCharges + bill.DeliveryFees;

            return bill;
        }
    }
}