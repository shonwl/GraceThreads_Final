namespace GraceThreads.Models
{
    public class AdminOrder
    {
        public string OrderId { get; set; } = string.Empty;
        public string Customer { get; set; } = string.Empty;
        public string Item { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Status { get; set; } = "Processing";
        public decimal Total { get; set; }
    }
}