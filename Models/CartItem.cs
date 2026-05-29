namespace untitled1.Models
{
    public class CartItem
    {
        public int MovieId { get; set; }
        public string MovieTitle { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => Price * Quantity;
    }
}
