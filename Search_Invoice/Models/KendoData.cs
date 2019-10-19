using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Search_Invoice.Models
{
    public class KendoData <T>
    {
        public List<T> data { get; set; }
        public int total { get; set; }
    }
}