using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraceThreads.Data;
using GraceThreads.Models;
using GraceThreads.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GraceThreads.Pages
{
    public class OrderConfirmationModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public OrderConfirmationModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public string OrderId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public List<CartItem> CartItems { get; set; } = new();
        public decimal Subtotal => CartItems.Sum(i => i.Price * i.Quantity);
        public decimal Shipping => Subtotal >= 1000 ? 0 : 55m;
        public decimal Total => Subtotal + Shipping;

        public async Task<IActionResult> OnGetAsync(string orderId)
        {
            if (string.IsNullOrEmpty(orderId))
            {
                return RedirectToPage("/Cart");
            }

            // 1. Pull order details straight out of database records
            var order = await _db.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null)
            {
                return RedirectToPage("/Cart");
            }

            // 2. Set UI bind items directly using your model property variables
            OrderId = order.OrderId;
            FullName = order.CustomerName;
            Email = order.CustomerEmail;
            Address = order.ShippingAddress;
            City = order.City;
            PostalCode = order.PostalCode;

            // 3. Mark checkout session process tracking state to processing
            if (order.Status == "Pending Payment")
            {
                order.Status = "Processing";
                await _db.SaveChangesAsync();
            }

            // 4. Populate loop item records mapping from relational OrderItems entries safely
            CartItems = await _db.OrderItems
                .Where(oi => oi.OrderId == orderId)
                .Include(oi => oi.Product)
                .Select(oi => new CartItem
                {
                    ProductName = oi.Product != null ? oi.Product.Name : "Product Item",
                    Variant = oi.Product != null ? oi.Product.Variant : "",
                    Price = oi.Price,
                    Quantity = oi.Quantity,
                    ImageUrl = oi.Product != null ? oi.Product.ImageUrl : "/images/placeholder.png"
                }).ToListAsync();

            // 5. Clean layout cache session elements
            CartService.Clear(HttpContext.Session);
            return Page();
        }
    }
}