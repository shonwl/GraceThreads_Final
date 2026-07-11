using GraceThreads.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GraceThreads.Pages.Admin
{
    public class ProductsModel : PageModel
    {
        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
            {
                return RedirectToPage("/Index");
            }
            return Page();
        }

        public IActionResult OnPostSave(int id, string name, string variant, decimal price, int stock, bool active)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
            {
                return RedirectToPage("/Index");
            }

            var product = AdminDataService.GetProduct(id);
            if (product != null)
            {
                product.Name = name;
                product.Variant = variant;
                product.Price = price;
                product.Stock = stock;
                product.Active = active;
            }
            return RedirectToPage();
        }
    }
}