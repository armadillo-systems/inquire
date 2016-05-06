using System.Collections.Generic;
using iNQUIRE.Models;

namespace iNQUIRE.ViewModels
{
    public class SearchAjaxViewModel
    {
        public SolrSearchResults SearchResults { get; set; }

        // eek, looks like an innocuous thing to do, but as this object gets serialized to JSON it results in the SearchResults huge field data being duplicated = JSON twice as big
        /*public List<IInqItem> Results
        {
            get { return SearchResults.Results; }
        }*/

        public int Rows { get; set; }
        public int RowStart { get; set; }

        public SearchAjaxViewModel(SolrSearchResults searchResults)
        {
            SearchResults = searchResults;
        }
    }
}