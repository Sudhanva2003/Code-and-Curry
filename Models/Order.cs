using System;
using System.Collections.Generic;

namespace Code_Curry.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int UserId { get; set; }

    public int RestId { get; set; }

    public DateTime? OrderDate { get; set; }

    public string? Status { get; set; }

    public decimal? TotalAmount { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual Restaurant Rest { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
