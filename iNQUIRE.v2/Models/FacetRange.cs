using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iNQUIRE.Models
{
    public class FacetRange
    {
        public string Field { get; set; }
        public string DisplayName { get; set; }
        public int Gap { get; set; }

        public int From { get; set; }
        public bool WildcardLower { get; set; }
        public int To { get; set; }
        public bool WildcardUpper { get; set; }

        public FacetRange(string field, string display_name, string from, string to, int gap)
        {
            Field = field;
            DisplayName = display_name;
            Gap = gap;

            if (from.Contains("*"))
            {
                WildcardLower = true;
                from = from.Remove(0, 1);
            }
            From = Convert.ToInt32(from);

            if (to.Contains("*"))
            {
                WildcardUpper = true;
                to = to.Remove(0, 1);
            }
            To = Convert.ToInt32(to);
        }
    }
}