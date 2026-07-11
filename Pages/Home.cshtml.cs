using GraceThreads.Models;
using GraceThreads.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GraceThreads.Pages
{
    public class HomeModel : PageModel
    {
        public string? WelcomeMessage { get; set; }
        public List<CartItem> CartItems { get; set; } = new();

        public void OnGet()
        {
            WelcomeMessage = TempData["WelcomeMessage"] as string;
            CartItems = CartService.GetCart(HttpContext.Session);
        }

        public IActionResult OnPostAddToCart(string productName, string variant, decimal price, string colorHex, string imageUrl)
        {
            CartService.AddItem(HttpContext.Session, new CartItem
            {
                ProductName = productName,
                Variant = variant,
                Price = price,
                Quantity = 1,
                ColorHex = colorHex,
                ImageUrl = imageUrl
            });
            return RedirectToPage();
        }
    }
}