using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Search_Invoice.Data.Domain
{
    public class inv_admin
    {
        [Key]
        public Guid inv_admin_id { get; set; }
        public string MST { get; set; }
        public string ConnectString { get; set; }
        public string Path { get; set; }
        public string alias { get; set; }
    }
}