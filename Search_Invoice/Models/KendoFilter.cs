using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Search_Invoice.Models
{
    public class KendoFilter
    {
        public string Logic { get; set; }
        public List<FilterOperator> Filters { get; set; }
    }

    public class FilterOperator
    {
        public string Value { get; set; }
        public string Field { get; set; }
        public string Operator { get; set; }
        public bool IgnoreCase { get; set; }

        public string SqlOperator
        {
            get
            {
                if (string.IsNullOrEmpty(Operator)) return "";

                string op = "";

                switch (Operator)
                {
                    case "contains":
                        op = "like N'%" + Value + "%'";
                        break;
                    case "startswith":
                        op = "like N'" + Value + "%'";
                        break;
                }

                return op;
            }
        }
    }
}