using System.ComponentModel.DataAnnotations;

namespace Code_Curry.DTOs
{
    public class RestaurantDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;
        [Required]
        [MaxLength(100)]
        public string Address { get; set; } = null!;
        [Required]
       public decimal? Rating { get; set; }
        [Required]
        [MaxLength(10)]
        public string? Phone { get; set; }
        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string? Email { get; set; }
        [Required]

        [MinLength(6)]
        public string? Password { get; set; }
        public bool IsOpen { get; set; } = true;
    }
}
