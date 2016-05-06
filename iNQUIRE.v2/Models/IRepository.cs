using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using SolrNet;

namespace iNQUIRE.Models
{
    public interface IRepository
    {
        string DjatokaResolverFile { get; set; }
        string ServerRootDirectory { get; set; }

        SolrSearchResults Search(SearchQuery query, List<KeyValuePair<string, string>> facets, List<FacetRange> facet_ranges);
        SolrSearchResults GetRecord(string id);
        SolrSearchResults GetSearchSuggestions(string str);

        bool UpdateFileFieldToJpeg2000(string id, string value);
        // String GetBaseUri(HttpRequestBase req, UrlHelper url_helper);

        XDocument XmlData { get; set; }
        string AppDataXml { get; set; }
        void Load();
        void Save();

        string MarkupCodedHyperlinks(string text);
        string MarkupNakedHyperlinks(string text);
    }
}