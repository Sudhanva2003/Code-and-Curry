using System;
using System.Collections.Generic;

namespace Code_Curry.Models;

public partial class User
{
    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string PasswordHash { get; set; } = null!;

    public string? Role { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
