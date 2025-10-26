using System;
using System.Collections.Generic;

namespace Code_Curry.Models;

public partial class SupportTicket
{
    public int TicketId { get; set; }

    public int? UserId { get; set; }

    public int? RestId { get; set; }

    public string Email { get; set; } = null!;

    public string Category { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTimeOffset CreatedDate { get; set; }

    public int? AssignedAdminId { get; set; }

    public string? AdminMessage { get; set; }

    public DateTimeOffset? ResolvedDate { get; set; }

    public string TicketStatus { get; set; } = null!;

    public virtual User? AssignedAdmin { get; set; }

    public virtual Restaurant? Rest { get; set; }

    public virtual User? User { get; set; }
}
