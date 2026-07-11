using GraceThreads.Models;

namespace GraceThreads.Services
{
    public static class AdminDataService
    {
        public static List<AdminProduct> Products { get; } = new()
        {
            new AdminProduct
            {
                Id = 1,
                Name = "Saved By Grace Tee",
                Variant = "Black — Ephesians 2:8",
                Category = "Tees",
                Description = "The flagship Grace Threads tee.",
                Price = 45,
                Stock = 24,
                Active = true,
                Tag = "New Drop",
                TagColorHex = "#f05a1a",
                ImageUrl = "/images/Black_Front.png"
            },
            new AdminProduct
            {
                Id = 2,
                Name = "Saved By Grace Tee",
                Variant = "White — Ephesians 2:8",
                Category = "Tees",
                Description = "The flagship Grace Threads tee.",
                Price = 45,
                Stock = 18,
                Active = true,
                Tag = "New Drop",
                TagColorHex = "#4ab4f0",
                ImageUrl = "/images/White_Front.png"
            }
        };

        public static List<AdminOrder> Orders { get; } = new()
        {
            new AdminOrder { OrderId = "#GT-00124", Customer = "John Grace",   Item = "Saved By Grace Tee — Black, S",  Date = DateTime.Now.AddDays(-1), Status = "Delivered",  Total = 45 },
            new AdminOrder { OrderId = "#GT-00123", Customer = "Sarah Faith",  Item = "Saved By Grace Tee — White, M",  Date = DateTime.Now.AddDays(-2), Status = "Shipped",    Total = 45 },
            new AdminOrder { OrderId = "#GT-00122", Customer = "Marcus King",  Item = "Saved By Grace Tee — Black",     Date = DateTime.Now.AddDays(-3), Status = "Processing", Total = 45 },
            new AdminOrder { OrderId = "#GT-00121", Customer = "Lydia Powell", Item = "Saved By Grace Tee — White, L",  Date = DateTime.Now.AddDays(-4), Status = "Delivered",  Total = 45 },
            new AdminOrder { OrderId = "#GT-00120", Customer = "Jason Reyes",  Item = "Saved By Grace Tee — Black, XL", Date = DateTime.Now.AddDays(-5), Status = "Delivered",  Total = 45 }
        };

        public static decimal TotalRevenue => Orders.Sum(o => o.Total);
        public static int TotalOrders => Orders.Count;
        public static int TotalProducts => Products.Count;
        public static int ActiveListings => Products.Count(p => p.Active);

        public static AdminProduct? GetProduct(int id) => Products.FirstOrDefault(p => p.Id == id);
    }
}