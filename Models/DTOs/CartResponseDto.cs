using System.Collections.Generic;

namespace untitled1.Models.DTOs
{
    public class CartResponseDto
    {
        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
        public int CartCount { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        
        // Formatted strings for direct frontend insertion if needed
        public string SubtotalFormatted => Subtotal.ToString("N0") + " VNĐ";
        public string TaxFormatted => Tax.ToString("N0") + " VNĐ";
        public string TotalFormatted => Total.ToString("N0") + " VNĐ";
    }
}
