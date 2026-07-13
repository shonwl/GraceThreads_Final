using GraceThreads.Data;
using GraceThreads.Models;
using GraceThreads.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraceThreads.Pages
{
    public class CatalogModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        // Inject the database context
        public CatalogModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public List<Product> Products { get; set; } = new();
        public List<CartItem> CartItems { get; set; } = new();

        public async Task OnGetAsync()
        {
            CartItems = CartService.GetCart(HttpContext.Session);
            
            // 1. Fetch products from DB
            // 2. Filter out inactive items
            // 3. Sort by newest created
            Products = await _db.Products
                .Where(p => p.Active)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
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
            var cart = CartService.GetCart(HttpContext.Session);
            var item = cart.FirstOrDefault(i => i.ProductName == productName && i.Variant == variant);
            
            if (item != null)
            {
                item.Quantity += amount;
                
                if (item.Quantity <= 0)
                {
                    cart.Remove(item);
                }
                
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