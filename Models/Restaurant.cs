using System;
using System.Collections.Generic;

namespace Code_Curry.Models;

public partial class Restaurant
{
    public int RestId { get; set; }

    public string Name { get; set; } = null!;

    public string Address { get; set; } = null!;

    public decimal? Rating { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string PasswordHash { get; set; } = null!;

    public bool IsOpen { get; set; }

    public virtual ICollection<Food> Foods { get; set; } = new List<Food>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
