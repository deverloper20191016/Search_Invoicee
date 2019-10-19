using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Search_Invoice.Data.Domain
{
    public class wb_user
    {
        [Key]
        [Column("wb_user_id")]
        public Guid id { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public Guid? wb_nhomquyen_id { get; set; }
        public string dien_giai { get; set; }
        public string email { get; set; }
        public string isviewuser { get; set; }
        public string isedituser { get; set; }
        public string isdeluser { get; set; }
        public string issigninvoice { get; set; }
        public bool? isLock { get; set; }
        public DateTime? timeLock { get; set; }
        public int? attempLogin { get; set; }
        public string ten_nguoi_sd { get; set; }

        [NotMapped]
        public string ma_dvcs { get; set; }
    }
}