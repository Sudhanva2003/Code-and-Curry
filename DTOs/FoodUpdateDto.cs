namespace Code_Curry.Dtos
{
    public class FoodUpdateDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? Category { get; set; }
        public bool IsAvailable { get; set; }
    }
}
