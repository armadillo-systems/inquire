using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
//using Ninject;
//using Ninject.Integration.SolrNet;
using SolrNet;
using SolrNet.Commands.Parameters;

namespace iNQUIRE.Models
{
    public class XmlSolrRespository : SolrRepository
    {
        public XmlSolrRespository()
        {
            _solrIdField = "id";
            _solrFileField = "file";
            _solrUri = ConfigurationManager.AppSettings["SolrUriXml"];
        }

        public override SolrSearchResults Search(SearchQuery query, List<KeyValuePair<string, string>> facets, List<FacetRange> facet_ranges)
        {
            // string solr_term = (ids.Count > 0) ? String.Format("{0}:^", _solrIdField) : makeSolrTerm(term, ids);
            string solr_term = makeSolrTerm(query.LanguageID, query.Term, query.IDs);

            var solr = ServiceLocator.Current.GetInstance<ISolrOperations<InqItemXml>>();
            var results = solr.Query(new SolrQuery(solr_term), new QueryOptions { Rows = query.Rows, Start = query.RowStart });

            var inqItems = new List<IInqItem>(results);
            // old: addImageMetaData(ref inqItems);

            var sr = new SolrSearchResults(inqItems)
            {
                Collapsing = results.Collapsing,
                FacetDates = results.FacetDates,
                FacetFields = results.FacetFields,
                FacetQueries = results.FacetQueries,
                Header = results.Header,
                Highlights = results.Highlights,
                MaxScore = results.MaxScore,
                NumFound = results.NumFound,
                SpellChecking = results.SpellChecking,
                Stats = results.Stats,
                SimilarResults = results.SimilarResults.ToDictionary(kvp => kvp.Key, kvp => new List<IInqItem>(kvp.Value))
            };
            return sr;
        }

        /* public override SolrSearchResults GetRecord(string id)
        {
            throw new NotImplementedException();
        }

        public override SolrSearchResults GetSearchSuggestions(string str)
        {
            throw new NotImplementedException();
        }*/
    }
}