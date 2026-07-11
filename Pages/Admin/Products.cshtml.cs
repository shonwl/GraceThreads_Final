using GraceThreads.Data;
using GraceThreads.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GraceThreads.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class ProductsModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public ProductsModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public List<Product> Products { get; private set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            Products = await _db.Products.OrderBy(p => p.Name).ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostSaveAsync(int id, string name, string variant, decimal price, int stock, bool active, string? imageUrl)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null)
            {
                // Create new product if id == 0 or not found
                product = new Product
                {
                    Id = id == 0 ? (await _db.Products.MaxAsync(p => (int?)p.Id) ?? 0) + 1 : id,
                    Name = name,
                    Variant = variant,
                    Price = price,
                    Stock = stock,
                    Active = active,
                    ImageUrl = imageUrl ?? string.Empty,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };
                _db.Products.Add(product);
            }
            else
            {
                product.Name = name;
                product.Variant = variant;
                product.Price = price;
                product.Stock = stock;
                product.Active = active;
                product.ImageUrl = imageUrl ?? product.ImageUrl;
                product.UpdatedAt = DateTimeOffset.UtcNow;
            }

            await _db.SaveChangesAsync();
            return RedirectToPage();
        }
    }
}
