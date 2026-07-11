using System.Text.Json;
using GraceThreads.Models;
using Microsoft.AspNetCore.Http;

namespace GraceThreads.Services
{
    public static class CartService
    {
        private const string SessionKey = "Cart";

        public static List<CartItem> GetCart(ISession session)
        {
            var json = session.GetString(SessionKey);
            if (string.IsNullOrEmpty(json))
                return new List<CartItem>();
            return JsonSerializer.Deserialize<List<CartItem>>(json) ?? new List<CartItem>();
        }

        private static void SaveCart(ISession session, List<CartItem> cart)
        {
            session.SetString(SessionKey, JsonSerializer.Serialize(cart));
        }

        public static void AddItem(ISession session, CartItem newItem)
        {
            var cart = GetCart(session);
            var existing = cart.FirstOrDefault(i => i.ProductName == newItem.ProductName && i.Variant == newItem.Variant);
            if (existing != null)
                existing.Quantity += newItem.Quantity;
            else
                cart.Add(newItem);
            SaveCart(session, cart);
        }

        public static void RemoveItem(ISession session, string productName, string variant)
        {
            var cart = GetCart(session);
            cart.RemoveAll(i => i.ProductName == productName && i.Variant == variant);
            SaveCart(session, cart);
        }

        public static void Clear(ISession session)
        {
            session.Remove(SessionKey);
        }
    }
}