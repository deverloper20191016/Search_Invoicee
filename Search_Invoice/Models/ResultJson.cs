using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Search_Invoice.Models
{
    public class ResultJson<T>
    {
        public string msg { get; set; }
        public T item { get; set; }
        public string description { get; set; }
    }
}