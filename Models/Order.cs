using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GraceThreads.Models
{
    public class Order
    {
        public string OrderId { get; set; } = string.Empty; // NVARCHAR(50) PK
        
        // 1. Made nullable (added the '?') so a user account is no longer forced
        public int? UserId { get; set; } 
        
        public DateTimeOffset Date { get; set; } = DateTimeOffset.UtcNow;
        public string Status { get; set; } = "Processing";
        public decimal Total { get; set; }

        // 2. Added Guest Checkout Details
        [Required]
        public string CustomerName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string CustomerEmail { get; set; } = string.Empty;
        
        [Required]
        public string ShippingAddress { get; set; } = string.Empty;
        
        [Required]
        public string City { get; set; } = string.Empty;
        
        [Required]
        public string PostalCode { get; set; } = string.Empty;

        // Navigation
        public User? User { get; set; }
        public List<OrderItem> Items { get; set; } = new();
    }
}