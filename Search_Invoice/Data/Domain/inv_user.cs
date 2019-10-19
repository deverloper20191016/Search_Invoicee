using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Search_Invoice.Data.Domain
{
    public class inv_user
    {
        [Key]
        public Guid inv_user_id { get; set; }
        public string mst { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string email { get; set; }
        public string ma_dt { get; set; }
        public DateTime? date_new { get; set; }
        public DateTime? date_edit { get; set; }
    }
}