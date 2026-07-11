using GraceThreads.Models;
using GraceThreads.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GraceThreads.Pages
{
    public class CheckoutModel : PageModel
    {
        public List<CartItem> CartItems { get; set; } = new();
        public decimal Subtotal => CartItems.Sum(i => i.Price * i.Quantity);
        public decimal Shipping => Subtotal >= 60 ? 0 : 6.99m;
        public decimal Total => Subtotal + Shipping;

        public IActionResult OnGet()
        {
            CartItems = CartService.GetCart(HttpContext.Session);
            if (!CartItems.Any())
            {
                return RedirectToPage("/Cart");
            }
            return Page();
        }
    }
}