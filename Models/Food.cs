using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Code_Curry.Models;

[Table("Food")]
public partial class Food
{
    [Key]
    public int FoodId { get; set; }

    public int RestId { get; set; }

    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(255)]
    public string? Description { get; set; }

    [Column(TypeName = "money")]
    public decimal Price { get; set; }

    [StringLength(50)]
    public string? Category { get; set; }

    [StringLength(20)]
    public string FoodStatus { get; set; } = null!;

    [StringLength(255)]
    public string? FoodImageUrl { get; set; }

    [InverseProperty("Food")]
    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    [ForeignKey("RestId")]
    [InverseProperty("Foods")]
    public virtual Restaurant Rest { get; set; } = null!;
}
