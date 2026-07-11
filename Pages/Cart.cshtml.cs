using GraceThreads.Models;
using GraceThreads.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GraceThreads.Pages
{
    public class CartModel : PageModel
    {
        public List<CartItem> CartItems { get; set; } = new();
        public decimal Total => CartItems.Sum(i => i.Price * i.Quantity);

        public void OnGet()
        {
            CartItems = CartService.GetCart(HttpContext.Session);
        }

        public IActionResult OnPostRemove(string productName, string variant)
        {
            CartService.RemoveItem(HttpContext.Session, productName, variant);
            return RedirectToPage();
        }

        public IActionResult OnPostClear()
        {
            CartService.Clear(HttpContext.Session);
            return RedirectToPage();
        }
    }
}