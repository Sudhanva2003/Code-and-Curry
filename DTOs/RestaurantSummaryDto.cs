using System.ComponentModel.DataAnnotations;

public class RestaurantSummaryDto
{
    [Required]
    public int RestId { get; set; }
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;
    [Required]
    public decimal? Rating { get; set; }
    [Required]
    public bool IsOpen { get; set; }
}
