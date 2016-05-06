using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iNQUIRE.Models
{
    public class SortField
    {
        public String DbField { get; set; }
        public String DisplayField { get; set; }
        public String SortDirection { get; set; }

        public SortField() { }

        public SortField(string db_field, string display_field, string sort_direction)
        {
            DbField = db_field;
            DisplayField = display_field;
            SortDirection = sort_direction;
        }
    }
}