using System.ComponentModel.DataAnnotations;

namespace Code_Curry.DTOs
{
    public class RestaurantEditDto
    {
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(255)]
        public string Address { get; set; } = null!;

        [MaxLength(15)]
        public string? Phone { get; set; }

        [MaxLength(100)]
        public string? Cuisine { get; set; }

        public string? RestImageUrl { get; set; }
    }
}
