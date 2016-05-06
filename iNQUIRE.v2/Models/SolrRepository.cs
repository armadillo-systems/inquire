using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using System.Linq;
using iNQUIRE.Helper;
using iNQUIRE.Helpers;
//using Ninject;
//using Ninject.Integration.SolrNet;
using SolrNet;
using SolrNet.Commands.Parameters;
using Microsoft.Practices.ServiceLocation;

namespace iNQUIRE.Models
{
    public abstract class SolrRepository : IRepository
    {
        public static string XmlPath { get; set; }
        public static bool MultiFacetConstraints { get; set; }
        public static int FacetLimit { get; set; }
        public static bool HyperlinkTargetBlank { get; set; }
        public static string NakedHyperlinkPrefix { get; set; }

        public static List<KeyValuePair<string, bool>> HyperlinkFields { get; set; }
        
        private static ISolrOperations<InqItemBodIIIF> _solr;
        private static ISolrOperations<InqItemBodIIIF> Solr
        {
            get
            {
                if (_solr == null)
                    _solr = ServiceLocator.Current.GetInstance<ISolrOperations<InqItemBodIIIF>>();

                return _solr;
            }
        }

        public XDocument XmlData { get; set; }

        public string AppDataXml { get; set; }
        public string DjatokaResolverFile { get; set; }
        public string ObjectIdFieldName { get; set; }
        public string ParentIdFieldName { get; set; }
        public string ServerRootDirectory { get; set; }

        protected string _solrUri;
        protected string _solrIdField;
        protected string _solrFileField;

        protected SolrRepository()
        {
            DjatokaResolverFile = ConfigurationManager.AppSettings["DjatokaResolverFile"];
            AppDataXml = ConfigurationManager.AppSettings["XmlDataFile"];
            ObjectIdFieldName = ConfigurationManager.AppSettings["ObjectIdFieldName"];
            ParentIdFieldName = ConfigurationManager.AppSettings["ParentIdFieldName"];
        }

        protected string makeSolrTerm(string term, List<string> ids)
        {
            if (!String.IsNullOrEmpty(term))
                return term; // search for "term" in the default field

            if ((ids != null) && ids.Count > 0)
            {
                var sb = new StringBuilder("(");

                int i = 0;
                foreach (string s in ids)
                {
                    sb.Append(_solrIdField);
                    sb.Append(":");
                    sb.Append(s); // .ToUpper()); needed in the past when using sql server as data store with solr, as guids would get stored in upper case in sql server, and lower case in solr 
                    i++;
                    if (i < ids.Count)
                        sb.Append(" OR ");
                }
                sb.Append(")");
                return sb.ToString(); // String.Format("{0}:", _solrIdField) + string.Join(" ", ids.ToArray());
            }

            return "*:*";
        }

        public void Load()
        {
            try
            {
                XmlData = XDocument.Load(String.Format("{0}{1}", XmlPath, AppDataXml));
            }
            catch (Exception e)
            {
                LogHelper.StatsLog(null, "Load()", String.Format("Fatal error, could not load xml data: {0}", e.Message), null, null);
            }
        }

        public void Save()
        {
            try
            {
                XmlData.Save(String.Format("{0}{1}", XmlPath, AppDataXml));
            }
            catch (Exception ex)
            {
                throw new Exception("XmlDataSave", ex.InnerException);
            }
        }

        //public String GetBaseUri(HttpRequestBase req, UrlHelper url_helper)
        //{
        //    if (req.Url != null)
        //    {
        //        return string.Format("{0}://{1}{2}", req.Url.Scheme, req.Url.Authority, url_helper.Content("~"));
        //    }

        //    LogHelper.StatsLog(null, "GetBaseUri()", String.Format("Failed, request is null!"), null, null);
        //    return "http://localhost/";
        //}

        // public abstract SolrSearchResults Search(string term, List<string> ids, int rows, int row_start, string parent_id, List<SortField> sort_orders, List<KeyValuePair<string, string>> facet_constraints, List<KeyValuePair<string, string>> facets, List<FacetRange> facet_ranges);
        // public abstract SolrSearchResults GetRecord(string id);
        // public abstract SolrSearchResults GetSearchSuggestions(string str);


        public virtual SolrSearchResults Search(SearchQuery query, List<KeyValuePair<string, string>> facets, List<FacetRange> facet_ranges)
        {
            // string solr_term = (ids.Count > 0) ? String.Format("{0}:^", _solrIdField) : makeSolrTerm(term, ids);
            string solr_term = makeSolrTerm(query.Term, query.IDs);

            //if (String.IsNullOrEmpty(query.ParentID))
            //    solr_term = String.Format("{0} AND -{1}:[* TO *]", solr_term, ParentIdFieldName);
            //else
            //    solr_term = String.Format("{0} AND {1}:{2}", solr_term, ParentIdFieldName, query.ParentID);

            if (String.IsNullOrEmpty(query.ParentID))
                solr_term = String.Format("{0} AND -{1}:[* TO *]", solr_term, ParentIdFieldName);
            else
                solr_term = String.Format("{0} AND {1}:{2}", solr_term, ParentIdFieldName, query.ParentID);


            #region facet constraints
            var str = new StringBuilder();
            var i = 0;
            foreach (FacetConstraint f in query.FacetConstraints) // KeyValuePair<string, List<string>>
            {
                int j = 0;
                str.Append("(");

                foreach (string val in f.Constraints)
                {
                    if (val.Contains(" to "))
                    {
                        // facet range, need to convert user friendly name sent by ajax back to correct db field name, if needed
                        var fn = f.Field;
                        foreach (FacetRange fr in facet_ranges)
                        {
                            if (fn == fr.DisplayName)
                            {
                                fn = fr.Field;
                                break;
                            }
                        }
                        // iterate over string list of values here, for multi valued constraints same key = OR, when going to different key AND
                        // if not multi valued constraints then always AND?
                        str.Append(String.Format("{0}:[{1}]", fn.Replace(":", @"\:"), val.ToUpper())); // str.Append("DateStartYear:[1800 TO 1900]");
                    }
                    else
                        str.Append(String.Format("{0}:{1}{2}{3}", f.Field.Replace(":", @"\:"), "\"", val, "\""));

                    j++;
                    if (j != f.Constraints.Count)
                        str.Append(" OR ");
                    else
                        str.Append(")");
                }

                i++;
                if (i != query.FacetConstraints.Count)
                {
                    //if(MultiFacetConstraints)
                    //    str.Append(" OR ");
                    //else
                        str.Append(" AND ");
                }
            }

            var d = new Dictionary<string, string> { { "fq", str.ToString() }, { "facet.sort", "count" } }; // ordering of the facets
            #endregion

            #region search term highlighting
            if (!String.IsNullOrEmpty(query.Term)) // if user has entered a search term then we want highlighting of that term in the results, if not we don't want it as adds overheads
            {
                d.Add("hl", "on");
                d.Add("hl.fl", "*");
                // d.Add("hl.fragsize", "0");
            }
            #endregion search term highlighting

            #region facet queries
            // only needed if not multi facet constraints, as multi facet constraints are enabled then we don't need to refine the 
            // available facets with the search (ie only return facets applicable to the search, the list obtained once at the start
            // remains valid, so no point adding overheads to this query
            // NB: if we are requesting 0 rows of results then this must be a request just for facet data, so in this case add the facet params
            var facet = new FacetParameters();
            facet.Limit = FacetLimit;

            if ((MultiFacetConstraints == false) || (query.Rows == 0))
            {
                var queries = new List<ISolrFacetQuery>();
                foreach (KeyValuePair<string, string> kvp in facets) // these come from the app config
                    queries.Add(new SolrFacetFieldQuery(kvp.Key));

                // have to use the slightly crude method below as Facet by Range is in Solr 3.1+ but not exposed by solrnet
                // see no FaceRanges property exposed by Solr results object
                // Would want to use following: &facet.range=DateStartYear&f.DateStartYear.facet.range.start=1000&f.DateStartYear.facet.range.end=2100&f.DateStartYear.facet.range.gap=50
                // Full query: http://armserv:8080/solr/core1/select?facet=true&facet.sort=count&start=0&q=*:*+AND+-ParentNodes:[*+TO+*]&?=&fq=&rows=0&facet.field=Genre&facet.range=DateStartYear&f.DateStartYear.facet.range.start=1000&f.DateStartYear.facet.range.end=2100&f.DateStartYear.facet.range.gap=50

                foreach (FacetRange fr in facet_ranges) // these need to come from the app config
                {
                    if (fr.WildcardLower)
                        queries.Add(new SolrFacetQuery(new SolrQueryByRange<string>(fr.Field, "*", (fr.From - 1).ToString())));

                    for (int y = fr.From; y < fr.To; y = y + fr.Gap)
                        queries.Add(new SolrFacetQuery(new SolrQueryByRange<string>(fr.Field, y.ToString(), (y + (fr.Gap - 1)).ToString())));

                    if (fr.WildcardUpper)
                        queries.Add(new SolrFacetQuery(new SolrQueryByRange<string>(fr.Field, (fr.To + 1).ToString(), "*")));
                }

                facet.Queries = queries;
            }
            #endregion

            #region sort orders
            var order_by = new List<SortOrder>();
            foreach (SortField sf in query.SortFields)
            {
                SortOrder so;

                if (sf.DbField == "Random")
                    so = new RandomSortOrder("randomF");
                else
                {
                    var order = Order.ASC;
                    if (sf.SortDirection.ToLower() == "desc")
                        order = Order.DESC;
                    so = new SortOrder(sf.DbField, order);
                }

                order_by.Add(so);
            }
            #endregion sort orders

            // i think the below could be put in super classes, and makeSolrSearchResults changed to accept the disassembled properties of the specific IInqItem implementation
            #region ninject approach doesn't work, get "can't create instance of interface" error?
            /*using (IKernel kernel = new StandardKernel(new SolrNetModule(_solrUri)))
            {
                var solr = kernel.Get<ISolrOperations<IInqItem>>();
                try
                {
                    var results = solr.Query(new SolrQuery(solr_term), new QueryOptions { Rows = rows, Start = row_start, Facet = facet, OrderBy = order_by, ExtraParams = d });
                    return makeSolrSearchResults(results, facets, facet_ranges);
                }
                catch (Exception e)
                {
                    throw new Exception("Fatal error, could not perform search, Solr not responding?: " + e.Message);
                }
            }*/
            #endregion

            // var solr = ServiceLocator.Current.GetInstance<ISolrOperations<InqItemArmNode>>();
            // var solr = ServiceLocator.Current.GetInstance<ISolrOperations<InqItemBod>>();
            
            //try
            //{
                var results = Solr.Query(new SolrQuery(solr_term), new QueryOptions
                {
                    Rows = query.Rows,
                    Start = query.RowStart,
                    Facet = facet,
                    OrderBy = order_by,
                    ExtraParams = d
                });
                return makeSolrSearchResults(results, facets, facet_ranges);
            //}
            //catch (Exception e)
            //{
            //    throw new Exception("Fatal error, could not perform search, Solr not responding or failed to create results?: " + e.Message);
            //}
        }

        /* ,
                    Highlight = new HighlightingParameters()
                    {
                        Fragsize = 100,
                        AfterTerm = "</em>",
                        BeforeTerm = "<em>",
                        Fields = new List<string>() { "Contents" },
                        Snippets = 5
                    }
         */

        public SolrSearchResults GetRecord(string id)
        {
            // var solr = ServiceLocator.Current.GetInstance<ISolrOperations<InqItemArmNode>>();
            // var solr = ServiceLocator.Current.GetInstance<ISolrOperations<InqItemBod>>();
            var results = Solr.Query(new SolrQuery(String.Format("{0}:{1}", ObjectIdFieldName, id)), new QueryOptions { Rows = 1, Start = 0 });
            return makeSolrSearchResults(results);
        }

        public SolrSearchResults GetSearchSuggestions(string str)
        {
            // var solr = ServiceLocator.Current.GetInstance<ISolrOperations<InqItemArmNode>>();
            // var solr = ServiceLocator.Current.GetInstance<ISolrOperations<InqItemBod>>();
            var results = Solr.Query(new SolrQuery(str), new QueryOptions { SpellCheck = new SpellCheckingParameters { } });
            return makeSolrSearchResults(results);
        }

        protected SolrSearchResults makeSolrSearchResults(SolrQueryResults<InqItemBodIIIF> results)
        //protected SolrSearchResults makeSolrSearchResults(SolrQueryResults<InqItemArmNode> results)
        // protected SolrSearchResults makeSolrSearchResults(SolrQueryResults<InqItemBase> results)
        {
            return makeSolrSearchResults(results, new List<KeyValuePair<string, string>>(), new List<FacetRange>());
        }

        protected SolrSearchResults makeSolrSearchResults(SolrQueryResults<InqItemBodIIIF> results, List<KeyValuePair<string, string>> facets, List<FacetRange> facet_ranges)
        //protected SolrSearchResults makeSolrSearchResults(SolrQueryResults<InqItemArmNode> results, List<KeyValuePair<string, string>> facets, List<FacetRange> facet_ranges)
        // protected SolrSearchResults makeSolrSearchResults(SolrQueryResults<InqItemBase> results, List<KeyValuePair<string, string>> facets, List<FacetRange> facet_ranges)
        {
            var inqItems = new List<IInqItem>(results);
            // old: addImageMetaData(ref inqItems);

            // need to replace the possibly very un-user-friendly database facet field names with more user friendly ones
            var facets_display = new Dictionary<string, ICollection<KeyValuePair<string, int>>>();

            foreach (KeyValuePair<string, ICollection<KeyValuePair<string, int>>> kvp in results.FacetFields)
            {
                foreach (KeyValuePair<string, string> f in facets)
                {
                    if (kvp.Key == f.Key)
                    {
                        facets_display.Add(String.Format("{0}^{1}", f.Key, f.Value), kvp.Value);
                        break;
                    }
                }
            }

            // same for facet ranges
            var facet_ranges_display = new Dictionary<string, int>();

            foreach (KeyValuePair<string, int> kvp in results.FacetQueries)
            {
                foreach (FacetRange fr in facet_ranges)
                {
                    // in format "DateEndYear:[1901 TO *]"
                    var s = kvp.Key.Split(new[] { ":[" }, StringSplitOptions.RemoveEmptyEntries);

                    if (s[0] == fr.Field)
                    {
                        facet_ranges_display.Add(String.Format("{0}^{1}{2}{3}", fr.Field, fr.DisplayName, ":[", s[1]), kvp.Value);
                        break;
                    }
                }
            }

            // hyperlink links in fields as appropriate
            if ((inqItems.Count > 0) && (HyperlinkFields.Count > 0))
            {
                System.Type inq_t = inqItems[0].GetType();

                foreach (KeyValuePair<string, bool> field in HyperlinkFields)                
                {
                    var p = inq_t.GetProperty(field.Key);
                    if (p != null)
                    {
                        foreach (IInqItem item in inqItems)
                        {
                            /* p.SetValue(item, "http://www.cnn.com/ This is a test http://www.bbc.co.uk , and this is another link http://moo.com/ this is some more text www.moo.com , ultimate link http://moo.com/ and more text.", null);
                            if (p.Name == "Description")
                                p.SetValue(item, "^start|http://www.bbc.co.uk^ This is a test. This sentence has a ^hyper link 1 title|http://www.bbc.co.uk^. This sentence does not. This sentence does ^|http://www.bbc.co.uk^, this is the final link ^final link|http://www.bbc.co.uk^ and that's it. ^|http://www.bbc.co.uk^", null);
                            */
                            string hyp_text;
                            var text = (string)p.GetValue(item, null);

                            if (field.Value)
                                hyp_text = MarkupCodedHyperlinks(text);
                            else
                                hyp_text = MarkupNakedHyperlinks(text);
                            
                            p.SetValue(item, hyp_text, null);
                        }
                    }
                }
            }

            var sr = new SolrSearchResults(inqItems)
            {
                Collapsing = results.Collapsing,
                FacetDates = results.FacetDates,
                Header = results.Header,
                Highlights = results.Highlights,
                MaxScore = results.MaxScore,
                NumFound = results.NumFound,
                SpellChecking = results.SpellChecking,
                Stats = results.Stats,
                SimilarResults = results.SimilarResults.ToDictionary(kvp => kvp.Key, kvp => new List<IInqItem>(kvp.Value)),
                FacetFields = facets_display,
                FacetQueries = facet_ranges_display
            };

            return sr;
        }

        public string MarkupCodedHyperlinks(string text)
        {
            // eg ^link title|http://www.bbc.co.uk^

            if (text == null)
                return text;

            if ((text.IndexOf("^")) == -1)
                return text; // nothing to do

            try
            {
                var html = new StringBuilder();
                var split = text.Split(new[] { "^" }, StringSplitOptions.None);

                foreach (string s in split)
                {
                    if (s.Contains("|"))
                    {
                        html.Append("<a href=\"");

                        var link_pair = s.Split(new[] { "|" }, StringSplitOptions.None);

                        html.Append(link_pair[1]);
                        html.Append("\"");

                        if (HyperlinkTargetBlank)
                            html.Append(" target=\"_blank\"");

                        html.Append(">");

                        if (string.IsNullOrEmpty(link_pair[0]))
                            html.Append(link_pair[1]);
                        else
                            html.Append(link_pair[0]);

                        html.Append("</a>");
                    }
                    else
                        html.Append(s);
                }

                return html.ToString();
            }
            catch (Exception e)
            {
                LogHelper.StatsLog(null, "MarkupCodedHyperlinks()", String.Format("Failed for {0}: {1}", text, e.Message), null, null);
                return text;
            }
        }

        public string MarkupNakedHyperlinks(string text)
        {
            if (text == null)
                return text;

            if (string.IsNullOrEmpty(NakedHyperlinkPrefix))
                return text;

            var urlstart = text.IndexOf(NakedHyperlinkPrefix);
            if (urlstart == -1)
                return text; // nothing to do

            try
            {
                var html = new StringBuilder();
                var urlend = 0;
                var substr = text;

                while (substr.Length > 0)
                {
                    var str = substr.Substring(0, urlstart);
                    html.Append(str);
                    html.Append("<a href=\"");

                    urlend = substr.IndexOf(" ", urlstart);

                    if (urlend == -1)
                        urlend = text.Length;

                    var url = substr.Substring(urlstart, urlend - urlstart);

                    if (NakedHyperlinkPrefix.ToLower().CompareTo("http://") != 0)
                        html.Append("http://");

                    html.Append(url);
                    html.Append("\"");

                    if (HyperlinkTargetBlank)
                        html.Append(" target=\"_blank\"");

                    html.Append(">");
                    html.Append(url);
                    html.Append("</a>");

                    substr = substr.Remove(0, urlend);

                    urlstart = substr.IndexOf(NakedHyperlinkPrefix);

                    if (urlstart == -1)
                    {
                        html.Append(substr); // no more url matches, just add the remaining text
                        break;
                    }
                }

                return html.ToString();
            }
            catch (Exception e)
            {
                LogHelper.StatsLog(null, "MarkupNakedHyperlinks()", String.Format("Failed for {0}: {1}", text, e.Message), null, null);
                return text;
            }
        }

        public virtual bool UpdateFileFieldToJpeg2000(string id, string value)
        {
            try
            {
                //using (IKernel kernel = new StandardKernel(new SolrNetModule(_solrUri)))
                //{
                var solr = ServiceLocator.Current.GetInstance<ISolrOperations<IInqItem>>(); // kernel.Get<ISolrOperations<IInqItem>>();
                    var items = solr.Query(new SolrQuery(String.Format("{0}:{1}", _solrFileField, id)));

                    if (items.Count == 1)
                    {
                        items[0].File = value;
                        solr.Add(items[0]);
                        solr.Commit();
                    }
                    else
                        throw new Exception(String.Format("Warning {0} items found to update for item {1}: {2}", items.Count, _solrIdField, id));
                //}

                LogHelper.StatsLog(null, "Solr Update()", String.Format("Updated Solr for {0}", id), null, null);
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.StatsLog(null, "Solr Update()", String.Format("Failed for {0}: {1}", id, ex.Message), null, null);
                return false;
            }
        }

        #region superceeded by solr search and update methods
        //public IEnumerable<InqItemXml> GetAllItemsFromXml()
        //{
        //    return SearchXml(null, null, null, false);
        //}
        //public static IEnumerable<InqItemXml> SearchXml(String base_uri, string term, List<string> ids, bool collection_search)
        //{
        //    var results = from j in _xmlData.Descendants("Jpeg2000")
        //                  select new InqItemXml()
        //                  {
        //                      ID = j.Element("ID").Value,
        //                      Title = j.Element("Title").Value,
        //                      Year = Convert.ToInt32(j.Element("Year").Value),
        //                      Description = j.Element("Description").Value,
        //                      File = j.Element("File").Value,
        //                      Author = j.Element("Author").Value,
        //                      Type = j.Element("Type").Value,
        //                      Subject = j.Element("Subject").Value,
        //                      Source = j.Element("Source").Value,
        //                      Collection = j.Element("Collection").Value
        //                  };

        //    if (!String.IsNullOrEmpty(term))
        //        results = results.Where(j => (j.Title.ToLower().Contains(term.ToLower()) || j.Author.ToLower().Contains(term.ToLower()) || (j.Description.ToLower().Contains(term.ToLower()))));
        //    else if ((ids != null) && ids.Count > 0)
        //        results = results.Where(j => ids.Contains(j.ID));
        //    else if (collection_search) 
        //        results = new List<InqItemXml>(); // need this case as normally a search with no params returns everything, but in the case of when viewing a collection no params should give no results

        //    var r_list = new List<InqItemXml>(results);

        //    var ser = new JavaScriptSerializer();

        //    if (String.IsNullOrEmpty(base_uri) == false) // add in image metadata, not required for Solr indexing
        //    {
        //        foreach (InqItemXml k in r_list)
        //        {
        //            if (k.File.Contains("info:"))
        //            {
        //                var md_str = DjatokaHelper.GetJpeg2000Metadata(base_uri, k.File, true);
        //                md_str = md_str.Replace(@"\", @"\\");
        //                var md = ser.Deserialize<Jpeg2000Metadata>(md_str);
        //                k.ImageMetadata = md;
        //            }
        //            else
        //            {
        //                // most importantly we need the width and height of non-jpeg2000 images so can re-size them to thumbnails whilst retaining the aspect ratio
        //                var d = ImageHelper.GetImageDimensions(k.File);
        //                k.ImageMetadata =
        //                    ser.Deserialize<Jpeg2000Metadata>(
        //                        String.Format(
        //                            "{{ 'identifier': '{0}', 'imagefile': '{0}', 'width': '{1}', 'height': '{2}', 'dwtLevels': '0', 'levels': '0', 'compositingLayerCount': '0' }}",
        //                            k.File, d[0], d[1]));
        //            }
        //        }
        //    }

        //    return r_list;
        //}

        //public bool UpdateXml(string element_name, string id, string value)
        //{
        //    try
        //    {
        //        XmlData.Descendants("Jpeg2000").Where(x => x.Element(element_name).Value == id).Single().SetElementValue(element_name, value);
        //        Save();
        //        LogHelper.StatsLog(null, "Update()", String.Format("Updated Xml for {0}", id), null, null);
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.StatsLog(null, "Update()", String.Format("Failed for {0}: {1}", id, ex.Message), null, null);
        //        return false;
        //    }
        //}
        #endregion

        // see: http://jqueryui.com/demos/autocomplete/#remote-with-cache
        // might help: http://blog.schuager.com/2008/09/jquery-autocomplete-json-apsnet-mvc.html
        //public static IEnumerable<string> GetAutoCompleteData(string uri)
        //{
        //    XDocument imgsXml = XDocument.Load(uri + _appDataXml);

        //    var results = from j in imgsXml.Descendants("Jpeg2000")
        //                  select new { 1, 2 } ;// { j.Element("Title") };
        //    return results;
        //}
    }
}