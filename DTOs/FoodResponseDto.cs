namespace Code_Curry.Dtos
{
    public class FoodResponseDto
    {
        public int FoodId { get; set; }
        public int RestId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? Category { get; set; }
        public bool IsAvailable { get; set; }

        // Include just the restaurant name, not all its foods
        public string RestaurantName { get; set; } = null!;
    }

}