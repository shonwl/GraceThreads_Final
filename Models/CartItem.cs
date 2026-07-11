namespace GraceThreads.Models
{
    public class CartItem
    {
        public string ProductName { get; set; } = string.Empty;
        public string Variant { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ColorHex { get; set; } = "#f05a1a";
        public string ImageUrl { get; set; } = "/images/Black_Front.png";
    }
}