using System;
using System.Collections.Generic;

namespace Code_Curry.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int UserId { get; set; }

    public int RestId { get; set; }

    public int? DelivererId { get; set; }

    public DateTimeOffset OrderDate { get; set; }

    public string Status { get; set; } = null!;

    public decimal TotalAmount { get; set; }

    public decimal? Discount { get; set; }

    public decimal HandlingFee { get; set; }

    public decimal PlatformFee { get; set; }

    public decimal DeliveryFee { get; set; }

    public decimal Gst { get; set; }

    public decimal FinalPrice { get; set; }

    public virtual User? Deliverer { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual Restaurant Rest { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
