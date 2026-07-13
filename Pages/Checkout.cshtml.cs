using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GraceThreads.Data;
using GraceThreads.Models;
using GraceThreads.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace GraceThreads.Pages
{
    public class CheckoutModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _config;

        public CheckoutModel(ApplicationDbContext db, IConfiguration config)
        {
            _db = db;
            // Added the assignment here so the configuration is saved
            _config = config;
        }

        public List<CartItem> CartItems { get; set; } = new();
        public decimal Subtotal => CartItems.Sum(i => i.Price * i.Quantity);
        public decimal Shipping => Subtotal >= 1000 ? 0 : 55m;
        public decimal Total => Subtotal + Shipping;

        public IActionResult OnGet()
        {
            CartItems = CartService.GetCart(HttpContext.Session);
            if (!CartItems.Any())
            {
                return RedirectToPage("/Cart");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string fullName, string email, string address, string city, string postalCode)
        {
            CartItems = CartService.GetCart(HttpContext.Session);
            if (!CartItems.Any())
            {
                return RedirectToPage("/Cart");
            }

            // Fallback validation check to keep [Required] model properties happy
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError(string.Empty, "Please fill out all required customer information fields.");
                return Page();
            }

            var orderId = "#GT-" + Random.Shared.Next(10000, 99999);

            int? userId = null;
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out var parsedId))
            {
                userId = parsedId;
            }

            // 1. Matches your Order model structure precisely
            var order = new Order
            {
                OrderId = orderId,
                UserId = userId, 
                Date = DateTimeOffset.UtcNow,
                Status = "Pending Payment",
                Total = Total,
                CustomerName = fullName,
                CustomerEmail = email,
                ShippingAddress = address ?? "N/A",
                City = city ?? "N/A",
                PostalCode = postalCode ?? "N/A"
            };
            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            // 2. Map items to OrderItem rows with accurate variant matching
            foreach (var item in CartItems)
            {
                // Match BOTH Name and Variant to get the exact primary key match (e.g., Black vs White tee)
                Product? matched = await _db.Products
                    .FirstOrDefaultAsync(p => p.Name == item.ProductName && p.Variant == item.Variant);
                
                // Fallback check: try matching just by name if the variant string is slightly off
                if (matched == null)
                {
                    matched = await _db.Products.FirstOrDefaultAsync(p => p.Name == item.ProductName);
                }

                var oi = new OrderItem
                {
                    OrderId = order.OrderId,
                    ProductId = matched?.Id, // Safely links your Product entity Id
                    Quantity = item.Quantity,
                    Price = item.Price,
                    LineTotal = item.Price * item.Quantity
                };
                _db.OrderItems.Add(oi);
            }
            await _db.SaveChangesAsync();

            // 3. Initiate PayMongo Session Pipeline
            using var client = new HttpClient();
            
            // Securely retrieves the API key from your appsettings.json file
            var secretKey = _config["PayMongo:SecretKey"]; 
            
            var authBytes = Encoding.ASCII.GetBytes($"{secretKey}:");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var payload = new
            {
                data = new
                {
                    attributes = new
                    {
                        send_email_receipt = false,
                        show_description = true,
                        show_line_items = true,
                        payment_method_types = new[] { "gcash" },
                        description = $"Grace Threads Order {orderId}",
                        line_items = CartItems.Select(item => new 
                        {
                            currency = "PHP",
                            amount = (int)(item.Price * 100),
                            description = item.Variant,
                            name = item.ProductName,
                            quantity = item.Quantity
                        }).ToList(),
                        
                        success_url = $"{baseUrl}/OrderConfirmation?orderId={Uri.EscapeDataString(orderId)}", 
                        cancel_url = $"{baseUrl}/Checkout"
                    }
                }
            };

            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var content = new StringContent(JsonSerializer.Serialize(payload, jsonOptions), Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync("https://api.paymongo.com/v1/checkout_sessions", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    using var jsonDoc = JsonDocument.Parse(responseString);
                    
                    var checkoutUrl = jsonDoc.RootElement
                        .GetProperty("data")
                        .GetProperty("attributes")
                        .GetProperty("checkout_url")
                        .GetString();
                    
                    if (!string.IsNullOrEmpty(checkoutUrl))
                    {
                        return Redirect(checkoutUrl);
                    }
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "An error occurred connecting to PayMongo.");
            }

            return Page();
        }
    }
}