using System;
using System.Collections.Generic;

namespace Code_Curry.Models;

public partial class Food
{
    public int FoodId { get; set; }

    public int RestId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public string? Category { get; set; }

    public bool IsAvailable { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual Restaurant Rest { get; set; } = null!;
}
