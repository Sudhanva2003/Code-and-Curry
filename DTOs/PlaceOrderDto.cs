namespace Code_Curry.DTOs
{
    public class PlaceOrderDto
    {

        public int UserId { get; set; }
        public List<OrderItemDto> OrderItems { get; set; } = new();
    }

        public class OrderItemDto
        {
            public int FoodId { get; set; }
            public int Quantity { get; set; }
        }

    
}
