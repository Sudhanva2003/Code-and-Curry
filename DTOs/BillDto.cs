namespace Code_Curry.DTOs
{

    public class BillDto
    {
        public List<BillItemDto> Items { get; set; } = new List<BillItemDto>();
        public decimal Subtotal { get; set; }
        public decimal SGST { get; set; }
        public decimal CGST { get; set; }
        public decimal HandlingCharges { get; set; }
        public decimal DeliveryFees { get; set; }
        public decimal FinalAmount { get; set; }
    }

    public class BillItemDto
    {
        public int FoodId { get; set; }
        public string FoodName { get; set; } = "";
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LinePrice { get; set; }
    }
}
