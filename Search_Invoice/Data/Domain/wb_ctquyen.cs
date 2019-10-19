using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Search_Invoice.Data.Domain
{
    [Table("wb_ctquyen")]
    public class wb_ctquyen
    {
        [Key]
        public Guid wb_ctquyen_id { get; set; }
        public Guid? wb_nhomquyen_id { get; set; }
        public Guid? wb_menu_id { get; set; }
        public string them { get; set; }
        public string sua { get; set; }
        public string xoa { get; set; }
    }
}