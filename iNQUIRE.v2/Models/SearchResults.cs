using System.Collections.Generic;
using System;
// using Ninject;
using SolrNet;
using SolrNet.Impl;

namespace iNQUIRE.Models
{
    public class SolrSearchResults
    {
        public List<IInqItem> Results { get; set; }
        public CollapseResults Collapsing { get; set; }
        public IDictionary<string, DateFacetingResult> FacetDates { get; set; }
        public IDictionary<string, ICollection<KeyValuePair<string,int>>> FacetFields { get; set; }
        public IDictionary<string, int> FacetQueries { get; set; }
        public ResponseHeader Header { get; set; }
        public IDictionary<string, HighlightedSnippets> Highlights { get; set; }
        // public IDictionary<string, IDictionary<string, ICollection<string>>> Highlights { get; set; }
        public double? MaxScore { get; set; }
        public int NumFound { get; set; }
        public IDictionary<string, List<IInqItem>> SimilarResults { get; set; }
        public SpellCheckResults SpellChecking { get; set; }
        public IDictionary<string, StatsResult> Stats { get; set; }
        public Guid SearchID { get; set; }
        public String SearchDisplayName { get; set; }
        public SearchQuery SearchQuery { get; set; }

        public SolrSearchResults(List<IInqItem> results)
        {
            Results = results;
        }
    }
}