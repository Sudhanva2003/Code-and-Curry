using System.ComponentModel.DataAnnotations;

namespace Code_Curry.DTOs
{
    public class SearchDto
    {
        [Required]
        public int RestId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Address { get; set; } = null!;

        [Required]
        public decimal? Rating { get; set; }

        [Required]
        public bool IsOpen { get; set; }
    }

    public class FoodSearchDto
    {
        [Required]
        public int FoodId { get; set; }

        [Required]
        public int RestId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(255)]
        public string? Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        [MaxLength(50)]
        public string? Category { get; set; }

        [Required]
        public bool IsAvailable { get; set; }
    }
}
