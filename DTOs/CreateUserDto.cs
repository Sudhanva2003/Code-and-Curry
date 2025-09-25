using System.ComponentModel.DataAnnotations;

namespace Code_Curry.DTOs
{
    public class CreateUserDto
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        [MaxLength(15)]
        public string Phone { get; set; }

        [MaxLength(255)]
        public string Address { get; set; }

        
        [MaxLength(10)]
        public string Role { get; set; }

        [Required]
        [MaxLength(255)]
        public string Password { get; set; }  // will be hashed before storing
    }
}
