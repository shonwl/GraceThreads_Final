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
    public class HomeModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        // Inject the database context
        public HomeModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public string? WelcomeMessage { get; set; }
        public List<Product> Products { get; set; } = new();
        public List<CartItem> CartItems { get; set; } = new();

        public async Task OnGetAsync()
        {
            WelcomeMessage = TempData["WelcomeMessage"] as string;
            CartItems = CartService.GetCart(HttpContext.Session);

            // Fetch active products ordered by newest
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
    }
}