using System.ComponentModel.DataAnnotations;

namespace Code_Curry.DTOs
{
    public class RestaurantDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [Required]
        [MaxLength(255)]
        public string Address { get; set; } = null!;


        [Required]
        [MaxLength(15)]
        public string Phone { get; set; }

        public string Cuisine { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        [MaxLength(15)]
        public string GstNo { get; set; } = null!;

        [Required]
        [MaxLength(14)]
        public string FssaiNo { get; set; } = null!;

        [MaxLength(20)]
        public string RestStatus { get; set; } = "Open";  // Open, Closed, Deleted

        public string? RestImageUrl { get; set; }
    }
}
