using System.ComponentModel.DataAnnotations;

namespace Code_Curry.DTOs
{
    public class RestaurantSummaryDto
    {
        public int RestId { get; set; }

        [MaxLength(100)]
        public string Name { get; set; } = null!;

        public decimal? Rating { get; set; }

        [MaxLength(20)]
        public string RestStatus { get; set; } = "Open";

        public string? RestImageUrl { get; set; }
    }
}
