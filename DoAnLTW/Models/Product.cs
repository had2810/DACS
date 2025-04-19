using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnLTW.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên sản phẩm không được quá 100 ký tự")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Giá sản phẩm là bắt buộc")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá sản phẩm không hợp lệ")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Thương hiệu là bắt buộc")]
        public int BrandId { get; set; }

        [ForeignKey("BrandId")]
        public Brand? Brand { get; set; }

        [Required(ErrorMessage = "Mô tả sản phẩm là bắt buộc")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Danh mục là bắt buộc")]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        // Một sản phẩm có nhiều hình ảnh
        public List<Product_Images> Images { get; set; } = new List<Product_Images>();

        // Liên kết với bảng trung gian ProductSize
        public List<ProductSize> ProductSizes { get; set; } = new List<ProductSize>();

        [NotMapped]
        public string ImageUrl { get; set; }

        // Thuộc tính tính toán tổng số lượng (nếu cần)
        [NotMapped]
        public int TotalStock => ProductSizes?.Sum(ps => ps.Stock) ?? 0;
    }
}