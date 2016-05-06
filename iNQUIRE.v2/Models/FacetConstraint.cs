using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iNQUIRE.Models
{
    public class FacetConstraint
    {
        public String Field { get; set; }
        // public String DisplayField { get; set; }
        public List<String> Constraints { get; set; }

        public FacetConstraint() { }

        public FacetConstraint(String field, List<String> constraints) // String display_field, 
        {
            Field = field;
            // DisplayField = display_field;
            Constraints = constraints;
        }
    }
}