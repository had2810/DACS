using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnLTW.Models
{
    public class Size
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Kích thước là bắt buộc")]
        [StringLength(30, ErrorMessage = "Kích thước không được quá 10 ký tự")]
        public string? size { get; set; } 

        public List<Product> Products { get; set; } = new List<Product>();
    }
}