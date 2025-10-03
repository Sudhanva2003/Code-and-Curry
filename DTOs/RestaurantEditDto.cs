using System.ComponentModel.DataAnnotations;

namespace Code_Curry.DTOs
{
    public class RestaurantEditDto
    {
        
            
            [MaxLength(100)]
            public string Name { get; set; } = null!;
            
            [MaxLength(100)]
            public string Address { get; set; } = null!;
           
            
           
            [MaxLength(10)]
            public string? Phone { get; set; }
            
            
           
           

           
            public bool IsOpen { get; set; } = true;
        }
    


}
