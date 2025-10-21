using System.ComponentModel.DataAnnotations;

namespace Code_Curry.DTOs
{
    public class RestaurantProfileDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [Required]
        [MaxLength(255)]
        public string Address { get; set; } = null!;

        [Required]
        public decimal? Rating { get; set; }

        [MaxLength(15)]
        public string? Phone { get; set; }

        [EmailAddress]
        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string RestStatus { get; set; } = "Open";

        [MaxLength(100)]
        public string? Cuisine { get; set; }

        public string? RestImageUrl { get; set; }

        [MaxLength(15)]
        public string GstNo { get; set; } = null!;

        [MaxLength(14)]
        public string FssaiNo { get; set; } = null!;
    }
}
