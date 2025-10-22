using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Code_Curry.Models;

public partial class Order
{
    [Key]
    public int OrderId { get; set; }

    public int UserId { get; set; }

    public int RestId { get; set; }

    public int? DelivererId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime OrderDate { get; set; }

    [StringLength(20)]
    public string Status { get; set; } = null!;

    [Column(TypeName = "money")]
    public decimal TotalAmount { get; set; }

    [Column(TypeName = "money")]
    public decimal? Discount { get; set; }

    [Column(TypeName = "money")]
    public decimal? HandlingFee { get; set; }

    [Column(TypeName = "money")]
    public decimal? PlatformFee { get; set; }

    [Column(TypeName = "money")]
    public decimal? DeliveryFee { get; set; }

    [Column(TypeName = "money")]

    public decimal? FinalPrice { get; set; }

    [Column(TypeName = "money")]
    public decimal? GST { get; set; }

    [ForeignKey("DelivererId")]
    [InverseProperty("OrderDeliverers")]
    public virtual User? Deliverer { get; set; }

    [InverseProperty("Order")]
    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    [ForeignKey("RestId")]
    [InverseProperty("Orders")]
    public virtual Restaurant Rest { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("OrderUsers")]
    public virtual User User { get; set; } = null!;
   
}
