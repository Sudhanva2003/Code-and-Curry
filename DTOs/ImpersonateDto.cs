namespace Code_Curry.Dtos
{
    public class ImpersonateDto
    {
        public string AdminEmail { get; set; } = null!;
        public string AdminPassword { get; set; } = null!;
        public string TargetEmail { get; set; } = null!;
    }
}
