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

            // Example calculations for new fields
            bill.Discount = 0;         // 0 by default
            bill.PlatformFee = subtotal * 0.02m;      // 2% platform fee
            bill.HandlingCharges = totalFoodItems * 5; // per item
            bill.DeliveryFees = 50;                    // flat delivery fee
            bill.SGST = (subtotal - bill.Discount) * 0.09m;
            bill.CGST = (subtotal - bill.Discount) * 0.09m;

            bill.FinalAmount = subtotal - bill.Discount + bill.SGST + bill.CGST +
                               bill.HandlingCharges + bill.DeliveryFees + bill.PlatformFee;

            return bill;
        }
    }
}
