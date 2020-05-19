using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using System.Text;

namespace iNQUIRE.Models
{
    public class SearchQuery
    {
        public Guid SearchID { get; set; }

        string _term;
        public string Term
        {
            get { return _term; }
            set
            {
                if (value == null)
                    value = "";
                _term = value;
            }
        }
        public List<string> IDs { get; set; }
        public int Rows { get; set; }
        public int RowStart { get; set; }
        public int NumFound { get; set; }
        public string ParentID { get; set; }
        public bool Empty { get; set; }
        public bool Hidden { get; set; }
        
        public List<SortField> SortFields { get; set; }

        public List<FacetConstraint> _facetConstraints;
        public List<FacetConstraint> FacetConstraints
        {
            get
            {
                _facetConstraints.Sort(delegate(FacetConstraint fc1, FacetConstraint fc2) { return fc1.Field.CompareTo(fc2.Field); });
                return _facetConstraints;
            }

            set
            {
                _facetConstraints = value;
            }
        }

        // public SortedDictionary<string, List<string>> FacetConstraintsDictionary { get; set; }

        public string UserID { get; set; }
        public string LanguageID { get; set; }

        public string DisplayName { get; set; }
        public DateTime Date { get; set; }

        public SearchQuery() { }

        public SearchQuery(string user_id, string term, string collection_ids, int rows, int row_start, string parent_id, string sort_orders, string facet_constraints, string lang_id)
        {
            if (String.IsNullOrEmpty(term) && (String.IsNullOrEmpty(facet_constraints)) && (String.IsNullOrEmpty(parent_id)))
                Empty = true;

            if (!String.IsNullOrEmpty(parent_id))
                Hidden = true;

            if (String.IsNullOrEmpty(user_id))
                UserID = String.Empty;
            else
                UserID = user_id;

            LanguageID = lang_id;
            Term = term;

            var ids = new string[0];
            if ((collection_ids != null) && (collection_ids.Contains("^")))
            {
                ids = collection_ids.Split(new[] { "^" }, StringSplitOptions.RemoveEmptyEntries);
                term = ""; // ignore any supplied search term, this might be changed in the future to allow sub searches of collections
            }

            IDs = new List<string>(ids);
            Rows = rows;
            RowStart = row_start;
            ParentID = parent_id;

            SortFields = decodeSortOrders(sort_orders);
            FacetConstraints = decodeFacetConstraints(facet_constraints);
            Date = DateTime.Now;
            
            // use some logic to create a user friendly name, to be displayed
            var sb = new StringBuilder();
            if(!String.IsNullOrEmpty(Term))
                sb.Append(String.Format("{0} ", Term));

            if((FacetConstraints != null) && (FacetConstraints.Count > 0))
            {
                if(!String.IsNullOrEmpty(Term))
                    sb.Append(" AND ");

                foreach (FacetConstraint fc in FacetConstraints)
                {
                    // sb.Append(fc.Field); we don't have the displayname...
                    int i = 0;
                    foreach(string c in fc.Constraints)
                    {
                        sb.Append(c);
                        i++;
                        if(i != fc.Constraints.Count)
                            sb.Append(" + ");
                    }
                }
            }
            var dn = sb.ToString();
            if (String.IsNullOrEmpty(dn))
                dn = "All";
            DisplayName = dn;
        }

        private List<SortField> decodeSortOrders(string sort_orders)
        {
            // sort orders and their directions
            var sort_fields = new List<SortField>();
            if ((String.IsNullOrEmpty(sort_orders) == false) && (sort_orders.Contains("^")))
            {
                var sort_orders_array = sort_orders.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                for (int j = 0; j < sort_orders_array.Length; j++)
                {
                    var sort_value_array = sort_orders_array[j].Split(new[] { "^" }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < sort_value_array.Length; i = i + 2)
                        sort_fields.Add(new SortField(sort_value_array[i], null, sort_value_array[i + 1]));
                }
            }
            return sort_fields;
        }

        private SortedDictionary<string, List<string>> decodeFacetConstraintsToDictionary(string facet_constraints)
        {
            // facets and their user applied values/constraints
            var facet_constraints_dict = new SortedDictionary<string, List<string>>();
            if ((String.IsNullOrEmpty(facet_constraints) == false) && (facet_constraints.Contains("^")))
            {
                var facet_constraints_array = facet_constraints.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                for (int j = 0; j < facet_constraints_array.Length; j++)
                {
                    var facet_value_array = facet_constraints_array[j].Split(new[] { "^" }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < facet_value_array.Length; i = i + 2)
                    {
                        // add facet to dictionary, (key -> facet name, value -> value), facet name will be facet_value_array[i], value will be facet_value_array[i + 1]
                        if (facet_constraints_dict.Any(x => x.Key == facet_value_array[i]))
                            facet_constraints_dict[facet_value_array[i]].Add(facet_value_array[i + 1]); // facet_constraints_dict.Keys.Contains< .Add(facet_value_array[i], facet_value_array[i + 1]);
                        else
                            facet_constraints_dict.Add(facet_value_array[i], new List<string>() { facet_value_array[i + 1] });
                    }
                }
            }
            return facet_constraints_dict;
        }

        private List<FacetConstraint> decodeFacetConstraints(string facet_constraints)
        {
            var sd = decodeFacetConstraintsToDictionary(facet_constraints);
            var fcs = new List<FacetConstraint>();

            foreach(KeyValuePair<string, List<string>> kvp in sd)
            {
                fcs.Add(new FacetConstraint(kvp.Key, kvp.Value));
            }

            return fcs;
        }
    }
}