using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Search_Invoice.Data.Domain
{
    public class wb_tab
    {
        [Key]
        [Column("wb_tab_id")]
        public Guid id { get; set; }
        public string tab_id { get; set; }
        public Guid? wb_window_id { get; set; }
        public string tab_name { get; set; }
        public string tab_table { get; set; }
        public string tab_type { get; set; }
        public string tab_view { get; set; }
        public string insert_cmd { get; set; }
        public string update_cmd { get; set; }
        public string delete_cmd { get; set; }
        public string select_cmd { get; set; }
        public int? stt { get; set; }
        public string foreign_key { get; set; }
        public string select_change { get; set; }
        public string prefix { get; set; }
        public string order_by { get; set; }
        public string cell_editstop { get; set; }
        public string after_addrow { get; set; }
        public string after_delrow { get; set; }
        public string oncheck { get; set; }
    }
}