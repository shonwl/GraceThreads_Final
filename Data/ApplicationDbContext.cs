using GraceThreads.Models;
using Microsoft.EntityFrameworkCore;

namespace GraceThreads.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Users
            modelBuilder.Entity<User>(b =>
            {
                b.ToTable("Users");
                b.HasKey(u => u.Id);
                b.Property(u => u.Id).ValueGeneratedOnAdd();
                b.HasIndex(u => u.Email).IsUnique();
                b.Property(u => u.RowVersion).IsRowVersion();
            });

            // Products
            modelBuilder.Entity<Product>(b =>
            {
                b.ToTable("Products");
                b.HasKey(p => p.Id);
                // Product ids are controlled by the business (seed uses specific ids)
                b.Property(p => p.Id).ValueGeneratedNever();
                b.Property(p => p.Price).HasColumnType("decimal(18,2)");
                b.Property(p => p.RowVersion).IsRowVersion();
            });

            // Orders
            modelBuilder.Entity<Order>(b =>
            {
                b.ToTable("Orders");
                b.HasKey(o => o.OrderId);
                b.Property(o => o.OrderId).HasMaxLength(50).ValueGeneratedNever();
                b.Property(o => o.Date).HasColumnType("datetimeoffset");
                b.Property(o => o.Total).HasColumnType("decimal(18,2)");

                b.HasOne(o => o.User)
                    .WithMany(u => u.Orders!)
                    .HasForeignKey(o => o.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // OrderItems
            modelBuilder.Entity<OrderItem>(b =>
            {
                b.ToTable("OrderItems");
                b.HasKey(oi => oi.Id);
                b.Property(oi => oi.Id).ValueGeneratedOnAdd();
                b.Property(oi => oi.Price).HasColumnType("decimal(18,2)");
                b.Property(oi => oi.LineTotal).HasColumnType("decimal(18,2)");

                b.HasOne(oi => oi.Order)
                    .WithMany(o => o.Items)
                    .HasForeignKey(oi => oi.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(oi => oi.Product)
                    .WithMany(p => p.OrderItems!)
                    .HasForeignKey(oi => oi.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
