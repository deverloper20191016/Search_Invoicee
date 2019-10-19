using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Search_Invoice.Data.Domain
{
    [Table("systemsetting")]
    public class systemsetting
    {
        [Key]
        public int systemsetting_id { get; set; }
        public string systemsetting_key { get; set; }
        public string systemsetting_value { get; set; }
        public Boolean? systemsetting_hide { get; set; }
        public Boolean? report { get; set; }
        public string dien_giai { get; set; }
    }
}