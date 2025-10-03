namespace Code_Curry.DTOs
{
    public class LoginDto
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class LoginResponseDto
    {
        public int UserId { get; set; }
        public string Role { get; set; } = null!;

        public string Name { get; set; }
    }
}
