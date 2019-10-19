using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Search_Invoice.Data.Domain
{
    public class wb_log_sms
    {
        [Key]
        public Guid wb_log_sms_id { get; set; }
        public string to_sdt { get; set; }
        public string ket_qua { get; set; }
        public string ky_hieu { get; set; }
        public DateTime? ngay_laphd { get; set; }
        public string so_hoadon { get; set; }
        public string so_bao_mat { get; set; }
    }
}