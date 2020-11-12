using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Search_Invoice.Data.Domain
{
    [Table("NguoiSuDung")]
    public class NguoiSuDung
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("TenTruyCap")]
        public string TenTruyCap { get; set; }

        [Column("MatKhau")]
        public string MatKhau { get; set; }

        [Column("TenNguoiDung")]
        public string TenNguoiDung { get; set; }

        [Column("MaQuyen")]
        public string MaQuyen { get; set; }

        [NotMapped]
        public string TenQuyen { get; set; }
    }
}