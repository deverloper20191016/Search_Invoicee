using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Search_Invoice.Data.Domain
{
    [Table("wb_log")]
    public class wb_log
    {
        [Key]
        public Guid id { get; set; }
        public DateTime datelog { get; set; }
        public string typelog { get; set; }
        public string ghichu { get; set; }
    }
}