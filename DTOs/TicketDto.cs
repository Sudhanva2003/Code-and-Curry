public class TicketDto
{
    public int TicketId { get; set; }
    public int? RestId { get; set; }
    public int? UserId { get; set; }
    public string Email { get; set; }
    public string Category { get; set; }
    public string Description { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public string TicketStatus { get; set; }
    public string Role { get; set; }
    public string AdminMessage { get; set; }
    public DateTimeOffset? ResolvedDate { get; set; }
    public int? AssignedAdminId { get; set; } 
}