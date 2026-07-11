using GraceThreads.Models;
using GraceThreads.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GraceThreads.Pages
{
    public class ProductModel : PageModel
    {
        public string ProductName { get; } = "Saved By Grace Tee";
        public string Color { get; set; } = "black";
        public string ColorLabel { get; set; } = "Black";
        public string ColorHex { get; set; } = "#1a1a1a";
        public string ImageFront { get; set; } = "/images/Black_Front.png";
        public List<string> Thumbnails { get; set; } = new();
        public decimal Price { get; } = 45m;
        public List<CartItem> CartItems { get; set; } = new();

        public void OnGet(string color = "black")
        {
            SetVariant(color);
            CartItems = CartService.GetCart(HttpContext.Session);
        }

        public IActionResult OnPostAddToCart(string color, int quantity, string size)
        {
            AddToCart(color, quantity, size);
            return RedirectToPage(new { color });
        }

        public IActionResult OnPostBuyNow(string color, int quantity, string size)
        {
            AddToCart(color, quantity, size);
            return RedirectToPage("/Cart");
        }

        private void AddToCart(string color, int quantity, string size)
        {
            SetVariant(color);
            if (quantity < 1) quantity = 1;

            CartService.AddItem(HttpContext.Session, new CartItem
            {
                ProductName = ProductName,
                Variant = $"{ColorLabel} — Ephesians 2:8 · Size {size}",
                Price = Price,
                Quantity = quantity,
                ColorHex = ColorHex,
                ImageUrl = ImageFront
            });
        }

        private void SetVariant(string color)
        {
            Color = string.Equals(color, "white", StringComparison.OrdinalIgnoreCase) ? "white" : "black";
            if (Color == "white")
            {
                ColorLabel = "White";
                ColorHex = "#f5f5f5";
                ImageFront = "/images/White_Front.png";
                Thumbnails = new List<string>
                {
                    "/images/White_Front.png",
                    "/images/White_Back.png",
                    "/images/SAVED_BLUE.png",
                    "/images/INNER.png",
                    "/images/Grace_Threads_Logo_white_1.png"
                };
            }
            else
            {
                ColorLabel = "Black";
                ColorHex = "#1a1a1a";
                ImageFront = "/images/Black_Front.png";
                Thumbnails = new List<string>
                {
                    "/images/Black_Front.png",
                    "/images/Black_Back.png",
                    "/images/SAVED_ORANGE.png",
                    "/images/INNER.png",
                    "/images/Grace_Threads_Logo_white_1.png"
                };
            }
        }
    }
}