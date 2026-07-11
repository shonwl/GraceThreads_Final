using GraceThreads.Models;
using GraceThreads.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GraceThreads.Pages
{
    public class CatalogModel : PageModel
    {
        public List<CartItem> CartItems { get; set; } = new();

        public void OnGet()
        {
            CartItems = CartService.GetCart(HttpContext.Session);
        }

        public IActionResult OnPostAddToCart(string productName, string variant, decimal price, string colorHex)
        {
            CartService.AddItem(HttpContext.Session, new CartItem
            {
                ProductName = productName,
                Variant = variant,
                Price = price,
                Quantity = 1,
                ColorHex = colorHex
            });
            return RedirectToPage();
        }
    }
}