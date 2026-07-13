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
        public IActionResult OnPostUpdateQuantity(string productName, string variant, int amount)
        {
            // 1. Pull the current items out of the session
            var cart = CartService.GetCart(HttpContext.Session);
            
            // 2. Locate the specific item matching the name and variant
            var item = cart.FirstOrDefault(i => i.ProductName == productName && i.Variant == variant);
            
            if (item != null)
            {
                // 3. Adjust quantity
                item.Quantity += amount;
                
                // 4. If quantity hits 0 or lower, drop it from the cart completely
                if (item.Quantity <= 0)
                {
                    cart.Remove(item);
                }
                
                // 5. Serialize and save the updated list back to the Session string
                HttpContext.Session.SetString("Cart", System.Text.Json.JsonSerializer.Serialize(cart));
            }

            return RedirectToPage();
        }

        public IActionResult OnPostRemove(string productName, string variant)
        {
            var cart = CartService.GetCart(HttpContext.Session);
            var item = cart.FirstOrDefault(i => i.ProductName == productName && i.Variant == variant);
            
            if (item != null)
            {
                cart.Remove(item);
                HttpContext.Session.SetString("Cart", System.Text.Json.JsonSerializer.Serialize(cart));
            }

            return RedirectToPage();
        }
        
    }
}