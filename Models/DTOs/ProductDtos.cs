using System.ComponentModel.DataAnnotations;

namespace untitled1.Models.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int Year { get; set; }
        public string Genre { get; set; } = string.Empty;
        public bool IsTVSeries { get; set; }
        public decimal Price { get; set; }
        public List<string> Categories { get; set; } = [];
    }

    public class CreateProductRequest
    {
        [Required(ErrorMessage = "Tên phim là bắt buộc.")]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = string.Empty;

        [Range(1900, 2100, ErrorMessage = "Năm phát hành không hợp lệ.")]
        public int Year { get; set; }

        public string Genre { get; set; } = string.Empty;

        public bool IsTVSeries { get; set; }

        [Range(0, 10_000_000, ErrorMessage = "Giá không hợp lệ.")]
        public decimal Price { get; set; } = 99000;

        public int[] CategoryIds { get; set; } = [];
    }

    public class UpdateProductRequest
    {
        [Required(ErrorMessage = "Tên phim là bắt buộc.")]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = string.Empty;

        [Range(1900, 2100, ErrorMessage = "Năm phát hành không hợp lệ.")]
        public int Year { get; set; }

        public string Genre { get; set; } = string.Empty;

        public bool IsTVSeries { get; set; }

        [Range(0, 10_000_000, ErrorMessage = "Giá không hợp lệ.")]
        public decimal Price { get; set; }

        public int[] CategoryIds { get; set; } = [];
    }
}
