using GraceThreads.Models;
using GraceThreads.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GraceThreads.Pages
{
    public class OrderConfirmationModel : PageModel
    {
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

        public IActionResult OnPost(string fullName, string email, string address, string city, string postalCode)
        {
            CartItems = CartService.GetCart(HttpContext.Session);
            if (!CartItems.Any())
            {
                return RedirectToPage("/Cart");
            }

            FullName = fullName;
            Email = email;
            Address = address;
            City = city;
            PostalCode = postalCode;
            OrderId = "#GT-" + Random.Shared.Next(10000, 99999);

            // Save the order details before clearing the cart
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