using System.ComponentModel.DataAnnotations;

public class FoodCreateDto
{
    [Required]                // must have RestId
    public int RestId { get; set; }

    [Required]
    [StringLength(100)]       // max 100 characters
    public string Name { get; set; }

   
    public string Description { get; set; }

    [Required]
    [Range(1, 10000)]          // Price must be between 1 and 10000
    public decimal Price { get; set; }

    [Required]
   
    public string Category { get; set; }

    public bool IsAvailable { get; set; } = true;
}
