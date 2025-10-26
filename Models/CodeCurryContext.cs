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

    public virtual DbSet<SupportTicket> SupportTickets { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:mycon");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Food>(entity =>
        {
            entity.HasKey(e => e.FoodId).HasName("PK__Food__856DB3EB919A964D");

            entity.ToTable("Food");

            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.FoodImageUrl).HasMaxLength(255);
            entity.Property(e => e.FoodStatus)
                .HasMaxLength(20)
                .HasDefaultValue("Available");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("money");

            entity.HasOne(d => d.Rest).WithMany(p => p.Foods)
                .HasForeignKey(d => d.RestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Food__RestId__2057CCD0");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Orders__C3905BCF3E372698");

            entity.Property(e => e.DeliveryFee).HasColumnType("money");
            entity.Property(e => e.Discount).HasColumnType("money");
            entity.Property(e => e.FinalPrice).HasColumnType("money");
            entity.Property(e => e.Gst)
                .HasColumnType("money")
                .HasColumnName("GST");
            entity.Property(e => e.HandlingFee).HasColumnType("money");
            entity.Property(e => e.OrderDate).HasDefaultValueSql("(sysdatetimeoffset())");
            entity.Property(e => e.PlatformFee).HasColumnType("money");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");
            entity.Property(e => e.TotalAmount).HasColumnType("money");

            entity.HasOne(d => d.Deliverer).WithMany(p => p.OrderDeliverers)
                .HasForeignKey(d => d.DelivererId)
                .HasConstraintName("FK__Orders__Delivere__57A801BA");

            entity.HasOne(d => d.Rest).WithMany(p => p.Orders)
                .HasForeignKey(d => d.RestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Orders__RestId__56B3DD81");

            entity.HasOne(d => d.User).WithMany(p => p.OrderUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Orders__UserId__55BFB948");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => e.OrderDetailId).HasName("PK__OrderDet__D3B9D36C5BC98A83");

            entity.ToTable("OrderDetail");

            entity.Property(e => e.Price).HasColumnType("money");

            entity.HasOne(d => d.Food).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.FoodId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderDeta__FoodI__5B78929E");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderDeta__Order__5A846E65");
        });

        modelBuilder.Entity<Restaurant>(entity =>
        {
            entity.HasKey(e => e.RestId).HasName("PK__Restaura__02F04D4A9D9E550E");

            entity.ToTable("Restaurant");

            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.Cuisine)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FssaiNo)
                .HasMaxLength(14)
                .IsUnicode(false);
            entity.Property(e => e.GstNo)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.Rating)
                .HasDefaultValue(4.0m)
                .HasColumnType("decimal(2, 1)");
            entity.Property(e => e.RestImageUrl).HasMaxLength(255);
            entity.Property(e => e.RestStatus)
                .HasMaxLength(20)
                .HasDefaultValue("Open");
        });

        modelBuilder.Entity<SupportTicket>(entity =>
        {
            entity.HasKey(e => e.TicketId).HasName("PK__SupportT__712CC6071E8C2B84");

            entity.ToTable("SupportTicket");

            entity.Property(e => e.AdminMessage).HasMaxLength(1000);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(sysdatetimeoffset())");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.TicketStatus)
                .HasMaxLength(20)
                .HasDefaultValue("Open");

            entity.HasOne(d => d.AssignedAdmin).WithMany(p => p.SupportTicketAssignedAdmins)
                .HasForeignKey(d => d.AssignedAdminId)
                .HasConstraintName("FK__SupportTi__Assig__68D28DBC");

            entity.HasOne(d => d.Rest).WithMany(p => p.SupportTickets)
                .HasForeignKey(d => d.RestId)
                .HasConstraintName("FK__SupportTi__RestI__67DE6983");

            entity.HasOne(d => d.User).WithMany(p => p.SupportTicketUsers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__SupportTi__UserI__66EA454A");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C315145FE");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534BC4382D4").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.LicenseNumber).HasMaxLength(50);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.Rating)
                .HasDefaultValue(4.0m)
                .HasColumnType("decimal(2, 1)");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasDefaultValue("Customer");
            entity.Property(e => e.UserStatus)
                .HasMaxLength(20)
                .HasDefaultValue("Active");
            entity.Property(e => e.VehicleNumber).HasMaxLength(20);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
