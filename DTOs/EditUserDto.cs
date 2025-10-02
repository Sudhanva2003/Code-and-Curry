using System.ComponentModel.DataAnnotations;

namespace Code_Curry.DTOs
{
    public class EditUserDto
    {
        
        [MaxLength(100)]
        public string FullName { get; set; }

        
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }

       
        [MaxLength(15)]
        public string Phone { get; set; }

        [MaxLength(255)]
        public string Address { get; set; }


        [MaxLength(10)]
        public string Role { get; set; }

        
        
    }
}
