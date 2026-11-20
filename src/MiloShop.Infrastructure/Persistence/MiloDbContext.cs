using MiloShop.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace MiloShop.Infrastructure.Persistence
{
    public class MiloDbContext : IdentityDbContext<ApplicationUser>
    {
        public MiloDbContext(DbContextOptions<MiloDbContext> options)
        : base(options) { }

        public DbSet<TestEntity> TestEntities { get; set; }
        public DbSet<SiteSetting> SiteSettings { get; set; } = default!;

        public DbSet<Product> Products { get; set; }
        // User & order flow
        public DbSet<Coupon> Coupons { get; set; } = null!;
        public DbSet<Offer> Offers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SiteSetting>()
                .HasIndex(x => x.Key)
                .IsUnique();

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Coupon>()
                .HasIndex(c => c.Code)
                .IsUnique();

            modelBuilder.Entity<Coupon>()
                .Property(c => c.Code)
                .HasConversion(v => v.Trim().ToUpper(), v => v);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.Orders)
                .WithOne()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.CartItems)
                .WithOne()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .HasOne(c => c.Product)
                .WithMany()
                .HasForeignKey(c => c.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .HasIndex(c => new { c.UserId, c.ProductId })
                .IsUnique();

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Coupon)
                .WithMany()
                .HasForeignKey(o => o.CouponId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Order>()
                .Property(o => o.RecipientFullName)
                .HasMaxLength(120);

            modelBuilder.Entity<Order>()
                .Property(o => o.RecipientPhoneNumber)
                .HasMaxLength(30);

            modelBuilder.Entity<Order>()
                .Property(o => o.ShippingAddress)
                .HasMaxLength(500);

            modelBuilder.Entity<Order>()
                .Property(o => o.PostalCode)
                .HasMaxLength(20);

            modelBuilder.Entity<Order>()
                .Property(o => o.CustomerNote)
                .HasMaxLength(800);

            modelBuilder.Entity<Order>()
                .Property(o => o.Status)
                .HasMaxLength(40);

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Order>()
                .Property(o => o.DiscountAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Order>()
                .Property(o => o.FinalAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasColumnType("decimal(18,2)");

        }
    }
}
