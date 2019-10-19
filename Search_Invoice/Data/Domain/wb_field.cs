using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Search_Invoice.Data.Domain
{
    public class wb_field
    {
        public Guid id { get; set; }
        public Guid? wb_tab_id { get; set; }
        public string column_name { get; set; }
        public int? row { get; set; }
        public int? col { get; set; }
        public int? width { get; set; }
        public int? stt { get; set; }
        public bool? hidden { get; set; }
        public int? column_width { get; set; }
        public int? label_width { get; set; }
        public string type_editor { get; set; }
        public string column_type { get; set; }
        public string label_position { get; set; }
        public Guid? ref_id { get; set; }
        public string list_column { get; set; }
        public string format { get; set; }
        public string space { get; set; }
        public string caption { get; set; }
        [Column("readonly")]
        public string read_only { get; set; }
        public string value_change { get; set; }
        public string default_value { get; set; }
        public bool? hide_in_grid { get; set; }
        public string value_suggest { get; set; }
        public string field_expression { get; set; }
        public string type_filter { get; set; }
        public string display_field { get; set; }
        public int? height { get; set; }
        public int? data_width { get; set; }
        public int? data_decimal { get; set; }
        public string styleinput { get; set; }
        public string css { get; set; }
        public string valid_rule { get; set; }
        public string total_column { get; set; }
        public string template { get; set; }
    }
}