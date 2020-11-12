using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Search_Invoice.Data.Domain
{
    [Table("DmQuyenHan")]
    public class QuyenHan
    {
        [Key]
        [Required(ErrorMessage = "Không được để trống")]
        [MaxLength(10, ErrorMessage = "Đối đa 10 ký tự")]
        [MinLength(2, ErrorMessage = "Tối đa 2 ký tự")]
        public string MaQuyen { get; set; }

        [Required(ErrorMessage = "Không được để trống")]
        [MaxLength(100, ErrorMessage = "Đối đa 10 ký tự")]
        [MinLength(3, ErrorMessage = "Tối đa 3 ký tự")]
        public string TenQuyen { get; set; }
    }
}