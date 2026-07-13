using GraceThreads.Data;
using GraceThreads.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GraceThreads.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class ProductsModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public ProductsModel(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public List<Product> Products { get; private set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            Products = await _db.Products.OrderBy(p => p.Name).ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostSaveAsync(int id, string name, string variant, decimal price, int stock, bool active, string? imageUrl, IFormFile? imageFile)
        {
            // 1. Process local file uploads if a file is present
            if (imageFile != null && imageFile.Length > 0)
            {
                const long maxFileSize = 2 * 1024 * 1024; // 2MB restriction checkpoint
                if (imageFile.Length > maxFileSize)
                {
                    TempData["ErrorMessage"] = "Server Upload Rejected: File size cannot cross the 2MB boundary.";
                    return RedirectToPage();
                }

                try
                {
                    // Target local workspace path: wwwroot/images/products
                    var targetFolder = Path.Combine(_env.WebRootPath, "images", "products");
                    if (!Directory.Exists(targetFolder))
                    {
                        Directory.CreateDirectory(targetFolder);
                    }

                    // Mask tracking with a clean GUID generation chain
                    var uniqueName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var fullStoragePath = Path.Combine(targetFolder, uniqueName);

                    using (var writeStream = new FileStream(fullStoragePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(writeStream);
                    }

                    // Reassign fallback target path to our newly created file
                    imageUrl = "/images/products/" + uniqueName;
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"System write error during upload processing: {ex.Message}";
                    return RedirectToPage();
                }
            }

            // 2. Fallback assignment logic if everything is blank
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                imageUrl = "/images/Grace_Threads_Logo_white_1.png";
            }

            // FIX: Convert null or blank variant spaces into an empty string to satisfy database NOT NULL rules
            if (string.IsNullOrWhiteSpace(variant))
            {
                variant = "";
            }

            // Only query the database if updating an existing record (id > 0)
            Product? product = null;
            if (id > 0)
            {
                product = await _db.Products.FindAsync(id);
            }

            if (product == null)
            {
                // Manually calculate the next sequential unique ID since DB auto-increment is missing
                int nextId = 1;
                if (await _db.Products.AnyAsync())
                {
                    nextId = await _db.Products.MaxAsync(p => p.Id) + 1;
                }

                // Create new dynamic record entry tracking mechanisms
                product = new Product
                {
                    Id = nextId, // Explicitly injecting our calculated unique ID
                    Name = name,
                    Variant = variant,
                    Price = price,
                    Stock = stock,
                    Active = active,
                    ImageUrl = imageUrl,
                    Category = "Tees", // Default safe placeholder category matching your DB records
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };
                _db.Products.Add(product);
            }
            else
            {
                // Update properties on active references
                product.Name = name;
                product.Variant = variant;
                product.Price = price;
                product.Stock = stock;
                product.Active = active;
                product.ImageUrl = imageUrl;
                product.UpdatedAt = DateTimeOffset.UtcNow;
            }

            await _db.SaveChangesAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product != null)
            {
                _db.Products.Remove(product);
                await _db.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}