using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Search_Invoice.Models
{
    public class PassModel
    {
        [Required]
        [Display(Name = "mst")]
        // [EmailAddress]
        public string mst { get; set; }

        [Required]
        [Display(Name = "username")]
        // [EmailAddress]
        public string username { get; set; }

        public string password { get; set; }

        public string renewpassword { get; set; }

        public string newpassword { get; set; }
    }
}