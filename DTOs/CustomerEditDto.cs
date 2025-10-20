using System.ComponentModel.DataAnnotations;

namespace Code_Curry.DTOs
{
    public class CustomerEditDto
    {
        
        [MaxLength(100)]
        public string FullName { get; set; }

        [MaxLength(15)]
        public string Phone { get; set; }

        [MaxLength(255)]
        public string Address { get; set; }

    }
}
