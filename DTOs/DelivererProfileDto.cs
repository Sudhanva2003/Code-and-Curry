namespace Code_Curry.DTOs
{
    public class DelivererProfileDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public string? Address { get; set; }

        public decimal? Rating { get; set; }
        public string LicenseNumber { get; set; } = null!;
        public string VehicleNumber { get; set; } = null!;
        public string UserStatus { get; set; } = null!;
    }
}
