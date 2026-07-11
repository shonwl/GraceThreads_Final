using GraceThreads.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace GraceThreads.Data
{
    public static class DbSeeder
    {
        // Seed some initial data if tables are empty. Keep minimal and idempotent.
        public static void Seed(ApplicationDbContext db, Microsoft.AspNetCore.Identity.IPasswordHasher<GraceThreads.Models.User>? passwordHasher = null)
        {
            // Ensure we have a password hasher available. If DI wasn't available during seeding,
            // create a local instance so we can still produce secure password hashes.
            if (passwordHasher == null)
            {
                passwordHasher = new Microsoft.AspNetCore.Identity.PasswordHasher<GraceThreads.Models.User>();
            }
            // Ensure at least one administrator exists
            var existingAdmin = db.Users.FirstOrDefault(u => u.Role == 0);
            if (existingAdmin == null)
            {
                var adminUser = new User { Email = "admin@gracethreads.com", DisplayName = "Administrator", Role = 0, CreatedAt = DateTimeOffset.UtcNow };
                adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "admin123");
                db.Users.Add(adminUser);
                db.SaveChanges();
            }
            else if (string.IsNullOrEmpty(existingAdmin.PasswordHash))
            {
                // If an admin row exists but has no password hash (possible if seeding ran before DI was available), update it.
                existingAdmin.PasswordHash = passwordHasher.HashPassword(existingAdmin, "admin123");
                db.SaveChanges();
            }

            // Seed demo customers only when none exist
            if (!db.Users.Any(u => u.Role == 1))
            {
                var johnHash = passwordHasher?.HashPassword(new User { Email = "john.grace@example.local" }, "Password123!") ?? string.Empty;
                var sarahHash = passwordHasher?.HashPassword(new User { Email = "sarah.faith@example.local" }, "Password123!") ?? string.Empty;

                db.Users.AddRange(new[]
                {
                    new User { Email = "john.grace@example.local", DisplayName = "John Grace", PasswordHash = johnHash, Role = 1, CreatedAt = DateTimeOffset.UtcNow },
                    new User { Email = "sarah.faith@example.local", DisplayName = "Sarah Faith", PasswordHash = sarahHash, Role = 1, CreatedAt = DateTimeOffset.UtcNow }
                });
                db.SaveChanges();
            }

            if (!db.Products.Any())
            {
                db.Products.AddRange(new[]
                {
                    new Product { Id = 1, Name = "Saved By Grace Tee", Variant = "Black — Ephesians 2:8", Category = "Tees", Description = "The flagship Grace Threads tee.", Price = 45m, Stock = 24, Active = true, Tag = "New Drop", TagColorHex = "#f05a1a", ImageUrl = "/images/Black_Front.png", CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow },
                    new Product { Id = 2, Name = "Saved By Grace Tee", Variant = "White — Ephesians 2:8", Category = "Tees", Description = "The flagship Grace Threads tee.", Price = 45m, Stock = 18, Active = true, Tag = "New Drop", TagColorHex = "#4ab4f0", ImageUrl = "/images/White_Front.png", CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow }
                });
                db.SaveChanges();
            }

            if (!db.Orders.Any())
            {
                var john = db.Users.FirstOrDefault(u => u.DisplayName == "John Grace");
                var sarah = db.Users.FirstOrDefault(u => u.DisplayName == "Sarah Faith");

                var orders = new[]
                {
                    new Order { OrderId = "#GT-00124", UserId = john?.Id ?? 0, Date = DateTimeOffset.Now.AddDays(-1), Status = "Delivered", Total = 45m },
                    new Order { OrderId = "#GT-00123", UserId = sarah?.Id ?? 0, Date = DateTimeOffset.Now.AddDays(-2), Status = "Shipped", Total = 45m }
                };

                db.Orders.AddRange(orders);
                db.SaveChanges();

                // Add order items
                var prod1 = db.Products.FirstOrDefault(p => p.Id == 1);
                if (prod1 != null)
                {
                    db.OrderItems.Add(new OrderItem { OrderId = "#GT-00124", ProductId = prod1.Id, Quantity = 1, Price = prod1.Price, LineTotal = prod1.Price });
                    db.OrderItems.Add(new OrderItem { OrderId = "#GT-00123", ProductId = prod1.Id, Quantity = 1, Price = prod1.Price, LineTotal = prod1.Price });
                    db.SaveChanges();
                }
            }
        }
    }
}
