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

}
