using System;
using System.Collections.Generic;

namespace Code_Curry.Models;

public partial class Restaurant
{
    public int RestId { get; set; }

    public string Name { get; set; } = null!;

    public string Address { get; set; } = null!;

    public decimal Rating { get; set; }

    public string? Cuisine { get; set; }

    public string Phone { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string GstNo { get; set; } = null!;

    public string FssaiNo { get; set; } = null!;

    public string RestStatus { get; set; } = null!;

    public string? RestImageUrl { get; set; }

    public virtual ICollection<Food> Foods { get; set; } = new List<Food>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<SupportTicket> SupportTickets { get; set; } = new List<SupportTicket>();
}
