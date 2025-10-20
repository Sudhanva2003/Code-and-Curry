using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Code_Curry.Models;

public partial class CodeCurryContext : DbContext
{
    public CodeCurryContext()
    {
    }

    public CodeCurryContext(DbContextOptions<CodeCurryContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Food> Foods { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<Restaurant> Restaurants { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=mycon");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Food>(entity =>
        {
            entity.HasKey(e => e.FoodId).HasName("PK__Food__856DB3EB58DA7A0B");

            entity.Property(e => e.FoodStatus).HasDefaultValue("Available");

            entity.HasOne(d => d.Rest).WithMany(p => p.Foods)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Food__RestId__19DFD96B");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Orders__C3905BCFBCA6BA17");

            entity.Property(e => e.OrderDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Status).HasDefaultValue("Pending");

            entity.HasOne(d => d.Deliverer).WithMany(p => p.OrderDeliverers).HasConstraintName("FK__Orders__Delivere__245D67DE");

            entity.HasOne(d => d.Rest).WithMany(p => p.Orders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Orders__RestId__236943A5");

            entity.HasOne(d => d.User).WithMany(p => p.OrderUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Orders__UserId__22751F6C");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => e.OrderDetailId).HasName("PK__OrderDet__D3B9D36CDB9E89B5");

            entity.HasOne(d => d.Food).WithMany(p => p.OrderDetails)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderDeta__FoodI__282DF8C2");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderDeta__Order__2739D489");
        });

        modelBuilder.Entity<Restaurant>(entity =>
        {
            entity.HasKey(e => e.RestId).HasName("PK__Restaura__02F04D4AA008BB99");

            entity.Property(e => e.RestStatus).HasDefaultValue("Open");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C31CAF139");

            entity.Property(e => e.Role).HasDefaultValue("Customer");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
