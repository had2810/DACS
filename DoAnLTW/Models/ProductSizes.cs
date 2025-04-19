using System.ComponentModel.DataAnnotations;

namespace DoAnLTW.Models
{
    public class ProductSize
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int SizeId { get; set; }
        public Size Size { get; set; }

        [Required(ErrorMessage = "Số lượng là bắt buộc")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn hoặc bằng 0")]
        public int Stock { get; set; } // Số lượng tồn kho cho kích thước này
    }
}