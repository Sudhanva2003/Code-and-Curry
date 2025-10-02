using System.ComponentModel.DataAnnotations;

namespace Code_Curry.DTOs
{
    public class RestaurantEditDto
    {
        
            
            [MaxLength(100)]
            public string Name { get; set; } = null!;
            
            [MaxLength(100)]
            public string Address { get; set; } = null!;
           
            public decimal? Rating { get; set; }
           
            [MaxLength(10)]
            public string? Phone { get; set; }
            
            [EmailAddress]
            [MaxLength(100)]
            public string? Email { get; set; }
           

           
            public bool IsOpen { get; set; } = true;
        }
    


}
