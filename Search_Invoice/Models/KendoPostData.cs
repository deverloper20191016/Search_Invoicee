using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Search_Invoice.Models
{
    public class KendoPostData
    {
        public KendoFilter Filter { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
    }
}