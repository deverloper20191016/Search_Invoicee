using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Search_Invoice.Data.Domain
{
    public class wb_menu
    {
        [Key]
        [Column("wb_menu_id")]
        public Guid id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public int? display_order { get; set; }
        public Guid? parentId { get; set; }
        public bool? status { get; set; }
        public string icon_css { get; set; }
        public string details { get; set; }
        public string code { get; set; }
        public string window_id { get; set; }
    }
}