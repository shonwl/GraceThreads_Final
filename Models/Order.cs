using System;
using System.Collections.Generic;

namespace GraceThreads.Models
{
    public class Order
    {
        public string OrderId { get; set; } = string.Empty; // NVARCHAR(50) PK
        public int UserId { get; set; }
        public DateTimeOffset Date { get; set; } = DateTimeOffset.UtcNow;
        public string Status { get; set; } = "Processing";
        public decimal Total { get; set; }

        // Navigation
        public User? User { get; set; }
        public List<OrderItem> Items { get; set; } = new();
    }
}
