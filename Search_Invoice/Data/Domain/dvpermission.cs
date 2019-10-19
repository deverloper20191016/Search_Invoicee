using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Search_Invoice.Data.Domain
{
    [Table("dvpermission")]
    public class dvpermission
    {
        [Key]
        public Guid dvpermission_id { get; set; }
        public Guid wb_user_id { get; set; }
        public Guid dmdvcs_id { get; set; }
    }
}