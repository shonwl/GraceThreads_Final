using System;

namespace GraceThreads.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public int? ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal LineTotal { get; set; }

        // Navigation
        public Order? Order { get; set; }
        public Product? Product { get; set; }
    }
}
