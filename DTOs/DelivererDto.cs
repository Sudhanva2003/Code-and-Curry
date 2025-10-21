using System.ComponentModel.DataAnnotations;

namespace Code_Curry.DTOs
{
    public class DelivererDto
    {
       
            [Required, StringLength(100)]
            public string FullName { get; set; }

            [Required, EmailAddress, StringLength(100)]
            public string Email { get; set; }

            [StringLength(15)]
            public string? Phone { get; set; }

            [StringLength(255)]
            public string? Address { get; set; }

            [Required, StringLength(255)]
            public string Password { get; set; }

            [StringLength(50)]
            public string? LicenseNumber { get; set; }

            [StringLength(20)]
            public string? VehicleNumber { get; set; }

        

    }
}
