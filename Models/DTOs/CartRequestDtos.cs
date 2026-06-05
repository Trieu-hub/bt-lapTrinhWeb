using System.ComponentModel.DataAnnotations;

namespace untitled1.Models.DTOs
{
    public class AddToCartRequest
    {
        [Required(ErrorMessage = "MovieId là bắt buộc.")]
        public int MovieId { get; set; }

        [Range(1, 100, ErrorMessage = "Số lượng phải nằm trong khoảng từ 1 đến 100.")]
        public int Quantity { get; set; } = 1;
    }

    public class UpdateCartRequest
    {
        [Range(1, 100, ErrorMessage = "Số lượng phải nằm trong khoảng từ 1 đến 100.")]
        public int Quantity { get; set; }
    }
}
