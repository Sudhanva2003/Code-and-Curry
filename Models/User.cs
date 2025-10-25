using System;
using System.Collections.Generic;

namespace Code_Curry.Models;

public partial class User
{
    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public decimal? Rating { get; set; }

    public string? Address { get; set; }

    public string PasswordHash { get; set; } = null!;

    public string UserStatus { get; set; } = null!;

    public string? Role { get; set; }

    public string? LicenseNumber { get; set; }

    public string? VehicleNumber { get; set; }

    public virtual ICollection<Order> OrderDeliverers { get; set; } = new List<Order>();

    public virtual ICollection<Order> OrderUsers { get; set; } = new List<Order>();

    public virtual ICollection<SupportTicket> SupportTicketAssignedAdmins { get; set; } = new List<SupportTicket>();

    public virtual ICollection<SupportTicket> SupportTicketUsers { get; set; } = new List<SupportTicket>();
}
