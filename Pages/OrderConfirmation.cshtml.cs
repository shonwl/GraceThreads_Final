using GraceThreads.Data;
using GraceThreads.Models;
using GraceThreads.Services;
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
        public decimal Shipping => Subtotal >= 60 ? 0 : 6.99m;
        public decimal Total => Subtotal + Shipping;

        public async Task<IActionResult> OnPostAsync(string fullName, string email, string address, string city, string postalCode)
        {
            CartItems = CartService.GetCart(HttpContext.Session);
            if (!CartItems.Any())
            {
                return RedirectToPage("/Cart");
            }

            // Require logged-in user for order creation (read from authentication claims)
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                // Redirect to login if user is not authenticated
                return RedirectToPage("/Index");
            }

            FullName = fullName;
            Email = email;
            Address = address;
            City = city;
            PostalCode = postalCode;
            OrderId = "#GT-" + Random.Shared.Next(10000, 99999);

            // Create order record
            var order = new Order
            {
                OrderId = OrderId,
                UserId = userId,
                Date = DateTimeOffset.UtcNow,
                Status = "Processing",
                Total = Total
            };

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            // Create order items and attempt to map to products by name/variant
            foreach (var item in CartItems)
            {
                Product? matched = null;
                try
                {
                    // Compute search token outside the EF expression to avoid expression-tree translation issues
                    var token = (item.Variant ?? string.Empty).Split(' ')[0];
                    matched = await _db.Products.FirstOrDefaultAsync(p => p.Name == item.ProductName && p.Variant.Contains(token));
                }
                catch { }

                var oi = new OrderItem
                {
                    OrderId = order.OrderId,
                    ProductId = matched?.Id,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    LineTotal = item.Price * item.Quantity
                };
                _db.OrderItems.Add(oi);
            }

            await _db.SaveChangesAsync();

            // Save last order info for UI and clear cart
            HttpContext.Session.SetString("LastOrderId", OrderId);
            HttpContext.Session.SetString("LastOrderName", fullName);

            CartService.Clear(HttpContext.Session);
            return Page();
        }

        public IActionResult OnGet()
        {
            // Prevent direct navigation without placing an order
            return RedirectToPage("/Cart");
        }
    }
}
