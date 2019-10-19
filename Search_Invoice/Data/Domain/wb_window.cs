using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Search_Invoice.Data.Domain
{
    [Table("wb_window")]
    public class wb_window
    {
        [Key]
        [Column("wb_window_id")]
        public Guid id { get; set; }
        public string window_id { get; set; }
        public string window_name { get; set; }
        public string window_type { get; set; }
        public int? width { get; set; }
        public int? height { get; set; }
        public int? rows_max { get; set; }
        public int? row_tab { get; set; }
        public string ma_ct { get; set; }
        public string after_new { get; set; }
        public string after_save { get; set; }
        public string sql_code { get; set; }
        //[NotMapped]
        public string layoutdetail { get; set; }
        public string before_save { get; set; }
        public string after_edit { get; set; }

        [Column("wb_infowindow_id")]
        public Guid? wb_infowindow_id { get; set; }
    }
}