using Marketplace.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Marketplace.Api.Infrastructure;

public class MarketplaceDbContext(DbContextOptions<MarketplaceDbContext> options) : DbContext(options)
{
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<FavoriteProduct> Favorites => Set<FavoriteProduct>();
    public DbSet<PickupPoint> PickupPoints => Set<PickupPoint>();
    public DbSet<OrderStatus> OrderStatuses => Set<OrderStatus>();
    public DbSet<DeliveryMethod> DeliveryMethods => Set<DeliveryMethod>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        var money = new ValueConverter<decimal, double>(v => (double)v, v => Math.Round((decimal)v, 2));

        model.Entity<Role>().HasIndex(r => r.Name).IsUnique();

        model.Entity<User>(e =>
        {
            e.HasIndex(u => u.Login).IsUnique();
            e.HasOne(u => u.Role).WithMany().HasForeignKey(u => u.RoleId).OnDelete(DeleteBehavior.Restrict);
        });

        model.Entity<Category>().HasIndex(c => c.Name).IsUnique();

        model.Entity<Product>(e =>
        {
            e.Property(p => p.Price).HasConversion(money);
            e.HasOne(p => p.Category).WithMany().HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Restrict);
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Products_Price", "Price >= 0");
                t.HasCheckConstraint("CK_Products_Stock", "Stock >= 0");
                t.HasCheckConstraint("CK_Products_PackSize", "PackSize IS NULL OR PackSize > 0");
            });
        });

        model.Entity<Review>(e =>
        {
            e.HasIndex(r => new { r.ProductId, r.UserId }).IsUnique();
            e.HasOne(r => r.Product).WithMany(p => p.Reviews).HasForeignKey(r => r.ProductId);
            e.HasOne(r => r.User).WithMany().HasForeignKey(r => r.UserId);
            e.ToTable(t => t.HasCheckConstraint("CK_Reviews_Rating", "Rating BETWEEN 1 AND 5"));
        });

        model.Entity<CartItem>(e =>
        {
            e.HasKey(c => new { c.UserId, c.ProductId });
            e.HasOne(c => c.User).WithMany().HasForeignKey(c => c.UserId);
            e.HasOne(c => c.Product).WithMany().HasForeignKey(c => c.ProductId);
            e.ToTable(t => t.HasCheckConstraint("CK_CartItems_Quantity", "Quantity > 0"));
        });

        model.Entity<FavoriteProduct>(e =>
        {
            e.HasKey(f => new { f.UserId, f.ProductId });
            e.HasOne(f => f.User).WithMany().HasForeignKey(f => f.UserId);
            e.HasOne(f => f.Product).WithMany().HasForeignKey(f => f.ProductId);
        });

        model.Entity<OrderStatus>().HasIndex(s => s.Name).IsUnique();
        model.Entity<DeliveryMethod>().HasIndex(d => d.Name).IsUnique();

        model.Entity<Order>(e =>
        {
            e.HasOne(o => o.User).WithMany().HasForeignKey(o => o.UserId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(o => o.Status).WithMany().HasForeignKey(o => o.StatusId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(o => o.DeliveryMethod).WithMany().HasForeignKey(o => o.DeliveryMethodId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(o => o.PickupPoint).WithMany().HasForeignKey(o => o.PickupPointId).OnDelete(DeleteBehavior.Restrict);
        });

        model.Entity<OrderItem>(e =>
        {
            e.Property(i => i.UnitPrice).HasConversion(money);
            e.HasOne(i => i.Order).WithMany(o => o.Items).HasForeignKey(i => i.OrderId);
            e.HasOne(i => i.Product).WithMany().HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Restrict);
            e.ToTable(t => t.HasCheckConstraint("CK_OrderItems_Quantity", "Quantity > 0"));
        });
    }
}
