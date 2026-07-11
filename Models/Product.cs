using System;
using System.Collections.Generic;

namespace GraceThreads.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Variant { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool Active { get; set; } = true;
        public string Tag { get; set; } = "New Drop";
        public string TagColorHex { get; set; } = "#f05a1a";
        public string ImageUrl { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
        public byte[]? RowVersion { get; set; }

        // Navigation
        public ICollection<OrderItem>? OrderItems { get; set; }
    }
}
