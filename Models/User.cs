using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Code_Curry.Models;

[Index("Email", Name = "UQ__Users__A9D10534B574DBC2", IsUnique = true)]
public partial class User
{
    [Key]
    public int UserId { get; set; }

    [StringLength(100)]
    public string FullName { get; set; } = null!;

    [StringLength(100)]
    public string Email { get; set; } = null!;

    [StringLength(15)]
    [Unicode(false)]
    public string? Phone { get; set; }

    [StringLength(255)]
    public string? Address { get; set; }

    [StringLength(255)]
    public string PasswordHash { get; set; } = null!;

    [StringLength(20)]
    public string? Role { get; set; }

    public string UserStatus { get; set; } = "Active";

    [Column(TypeName = "decimal(2, 1)")]
    public decimal? Rating { get; set; } = 4.0m;

    [StringLength(50)]
    public string? LicenseNumber { get; set; }

    [StringLength(20)]
    public string? VehicleNumber { get; set; }

    [InverseProperty("Deliverer")]
    public virtual ICollection<Order> OrderDeliverers { get; set; } = new List<Order>();

    [InverseProperty("User")]
    public virtual ICollection<Order> OrderUsers { get; set; } = new List<Order>();
}
