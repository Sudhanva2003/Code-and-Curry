using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Code_Curry.Models;

[Table("Restaurant")]
public partial class Restaurant
{
    [Key]
    public int RestId { get; set; }

    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(255)]
    public string Address { get; set; } = null!;

    [Column(TypeName = "decimal(2, 1)")]
    public decimal Rating { get; set; } = 4.0m;

    [StringLength(15)]
    [Unicode(false)]
    public string Phone { get; set; } = null!;

    [StringLength(100)]
    public string? Cuisine { get; set; }

    [StringLength(100)]
    public string Email { get; set; } = null!;

    [StringLength(255)]
    public string PasswordHash { get; set; } = null!;

    [StringLength(15)]
    [Unicode(false)]
    public string GstNo { get; set; } = null!;

    [StringLength(14)]
    [Unicode(false)]
    public string FssaiNo { get; set; } = null!;

    [StringLength(20)]
    public string RestStatus { get; set; } = null!;

    [StringLength(255)]
    public string? RestImageUrl { get; set; }

    [InverseProperty("Rest")]
    public virtual ICollection<Food> Foods { get; set; } = new List<Food>();

    [InverseProperty("Rest")]
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
