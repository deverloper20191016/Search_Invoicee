using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Search_Invoice.Data.Domain
{
    public class dmdvcs
    {
        [Key]
        public Guid dmdvcs_id { get; set; }
        public string ma_dvcs { get; set; }
        public string ten_dvcs { get; set; }
        public string ten_en { get; set; }
        public Guid? dvcs_me_id { get; set; }
        public bool? nh_cuoi { get; set; }
        public string ms_thue { get; set; }
        public string dia_chi { get; set; }
        public string dien_thoai { get; set; }
        public string fax { get; set; }
        public string email { get; set; }
        public string user_new { get; set; }
        public DateTime? date_new { get; set; }
        public string user_edit { get; set; }
        public DateTime? date_edit { get; set; }
        public string database_code { get; set; }
        public bool? chon { get; set; }
        public string giam_doc { get; set; }
        public string tai_khoan { get; set; }
        public string tai_nh { get; set; }
        public string qhns { get; set; }
        public string quan_huyen { get; set; }
        public string tinh_thanh { get; set; }
        public string nguoi_dai_dien { get; set; }
        public string chuc_vu { get; set; }
        public string ngay_bd_nam_tc { get; set; }
        public string mst_dvcq { get; set; }
        public string ten_dvcq { get; set; }
        public string email_dvcq { get; set; }
        public string ma_cqthuecaptinh { get; set; }
        public string ma_cqthuecapql { get; set; }
    }
}