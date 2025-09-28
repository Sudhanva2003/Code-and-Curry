using System.ComponentModel.DataAnnotations;

namespace Code_Curry.DTOs
{
    public class MenuItemDto
    {
        [Required]
            public int FoodId { get; set; }
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;
        [Required]
        [MaxLength(100)]
        public string Category { get; set; } = null!;
        [Required]
        [MaxLength(100)]
        public string Description { get; set; } = null!;
        [Required]
        [MaxLength(100)]
        public decimal Price { get; set; }
        [Required]
            public bool IsAvailable { get; set; }
        

    }
}
