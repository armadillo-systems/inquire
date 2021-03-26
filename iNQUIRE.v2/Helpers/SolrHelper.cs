using System;
using System.Configuration;
using System.Collections.Generic;
using System.Text;
using iNQUIRE.Models;
//using Microsoft.Practices.ServiceLocation;
//using Ninject;
using SolrNet;
using SolrNet.Exceptions;
using SolrNet.Impl;
using System.Reflection;
using System.Linq;
using System.Web.Script.Serialization;
using iNQUIRE.Helper;
using CommonServiceLocator;

namespace iNQUIRE.Helpers
{
    public static class SolrHelper
    {
        public static string MultilingualPropertyPrefix = "_ml";

        //private static readonly Helper.IJP2Helper _IJP2Helper;
        //private static readonly IRepository _IRepository;

        // if colons exist in the solr field names then we will have converted these manually
        // in the IInqItem implementation to underscores, so have to change them back in the
        // javascript code (field names can't have underscores AND colons because of this)
        public static bool ColonInFieldNames { get; set; }
        public static string FieldConcatenationString { get; set; }
        public static string ConcatenateFields { get; set; }
        private static bool _isSetup;

        public static string FacetsString
        {
            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    var facets_array = value.Split(new[] { "^" }, StringSplitOptions.RemoveEmptyEntries);

                    var facets = new List<KeyValuePair<string, string>>();
                    var facet_ranges = new List<FacetRange>();

                    for (int i = 0; i < facets_array.Length; i++)
                    {
                        var f = facets_array[i].Trim();
                        var f_array = f.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);

                        if (f_array.Length == 5)
                            facet_ranges.Add(new FacetRange(f_array[0], f_array[1], f_array[2], f_array[3], Convert.ToInt32(f_array[4])));
                        else if (f_array.Length == 2)
                            facets.Add(new KeyValuePair<string, string>(f_array[0], f_array[1]));
                        else
                            throw new Exception(String.Format("Config error, \"Facets\" key not in correct format: {0}", value));
                    }

                    Facets = facets;
                    FacetRanges = facet_ranges;
                }
            }
        }

        private static string _sortFieldsString;
        public static string SortFieldsString
        {
            get { return _sortFieldsString; }

            set
            {
                _sortFieldsString = value;

                if (!String.IsNullOrEmpty(value))
                {
                    SortFields = new List<SortField>();

                    var sort_fields_array = value.Split(new[] { "^" }, StringSplitOptions.RemoveEmptyEntries);
                    var sort_fields = new List<SortField>();

                    for (int i = 0; i < sort_fields_array.Length; i++)
                    {
                        var f = sort_fields_array[i].Trim();
                        var f_array = f.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);

                        if (f_array.Length == 3)
                            SortFields.Add(new SortField(f_array[0], f_array[1], f_array[2]));
                        else
                            throw new Exception(String.Format("Config error, \"SortFields\" key not in correct format: {0}", value));
                    }
                }
            }
        }

        public static List<KeyValuePair<string, string>> Facets { get; set; }
        public static List<FacetRange> FacetRanges { get; set; }
        public static List<SortField> SortFields { get; set; }


        static SolrHelper()
        {
            Setup();
        }

        public static void Setup()
        {
            if (!_isSetup)
            {
                try
                {
                    //_IJP2Helper = ijp2helper;
                    //_IRepository = irepository;
                    Facets = new List<KeyValuePair<string, string>>();
                    FacetRanges = new List<FacetRange>();


                    ColonInFieldNames = Convert.ToBoolean(ConfigurationManager.AppSettings["ColonInFieldNames"]);
                    FieldConcatenationString = ConfigurationManager.AppSettings["FieldConcatenationString"];
                    ConcatenateFields = ConfigurationManager.AppSettings["ConcatenateFields"];
                }
                catch (Exception e)
                {
                    iNQUIRE.Helper.LogHelper.StatsLog(null, String.Format("SolrHelper config error, check web.config parameters ColonInFieldNames: {0}, FieldConcatenationString: {1}, ConcatenateFields: {2}", ColonInFieldNames, FieldConcatenationString, ConcatenateFields), String.Format("Error {0}", e.Message), null, null);
                    ColonInFieldNames = false;
                }
                var solr_uri = ConfigurationManager.AppSettings["SolrUri"];

                //var connection1 = new SolrConnection(solr_uri);
                //var loggingConnection1 = new LoggingConnection(connection1);
                //Startup.Init<InqItemXml>(loggingConnection1);

                //using (IKernel kernel = new StandardKernel())
                //{
                //    kernel.Bind<IInqItem>()
                //        .To<InqItemXml>()
                //        .InSingletonScope();
                //    kernel.Load(new SolrNetModule(solr_uri));
                //}

                _isSetup = true;
            }
        }

        /// <summary>
        /// Adds some sample documents to Solr
        /// </summary>
        public static void AddInitialDocumentsFromXml(string solrUrl)
        {
            //try
            //{
            //    // var solr = ServiceLocator.Current.GetInstance<ISolrOperations<InqItemXml>>();

            //    using (IKernel kernel = new StandardKernel())
            //    {
            //        kernel.Bind<IInqItem>()
            //            .To<InqItemXml>()
            //            .InSingletonScope();
            //        kernel.Load(new SolrNetModule(solrUrl));
            //        var solr = ServiceLocator.Current.GetInstance<ISolrOperations<InqItemXml>>();

            //        solr.Delete(SolrQuery.All);
            //        solr.Commit();

            //        var items = XmlDataHelper.GetAllItemsFromXml();

            //        foreach (var item in items)
            //            solr.Add(item);
            //        solr.Commit();

            //        //solr.Add(new InqItemXml { Title = "Kodak EasyShare", ID = "17F595AF-293C-46F3-AB84-79A39FD579B5" });

            //        //var connection = ServiceLocator.Current.GetInstance<ISolrConnection>();
            //        //foreach (var file in Directory.GetFiles("", "*.xml"))
            //        //{
            //        //    connection.Post("/update", File.ReadAllText(file, Encoding.UTF8));
            //        //}
            //        //solr.Commit();
            //        //solr.BuildSpellCheckDictionary();
            //    }
            //}
            //catch (SolrConnectionException e)
            //{
            //    throw new Exception(string.Format("Solr {0}, error: {1}", solrUrl, e.Message));
            //}
        }

        public static void ForceHttps(ref SolrSearchResults solr_results)
        {
            foreach (IInqItem k in solr_results.Results)
            {
                var k2 = k as InqItemIIIFBase;
                if (k2 != null)
                {
                    k2.IIIFImageRoot = k2.IIIFImageRoot.Replace("http://", "https://");
                    k2.IIIFManifest = k2.IIIFManifest.Replace("http://", "https://");
                }
            }
        }

        public static void AddImageMetaData(ref SolrSearchResults solr_results, IJP2Helper _IJP2Helper)
        {
            var ser = new JavaScriptSerializer();

            //if (String.IsNullOrEmpty(base_uri) == false) // add in image metadata, not required for Solr indexing
            //{
            foreach (IInqItem k in solr_results.Results)
            {
                if (k.GetType().IsSubclassOf(typeof(InqItemIIIFBase)))
                {
                    // new logic here for IIIF images
                    // k.ImageMetadata = new ImageMetadata { Identifier = file, Imagefile = file, Width = d[0], Height = d[1], DwtLevels = 0, Levels = 0, CompositingLayerCount = 0 };
                }
                else
                {
                    if (k.File != null)
                    {
                        if (!String.IsNullOrEmpty((ImageHelper.Jpeg2000NamespaceReplace)) && k.File.Contains(ImageHelper.Jpeg2000NamespaceReplace)) // see Jpeg2000NamespaceReplace for explanation
                            k.File = k.File.Replace(ImageHelper.Jpeg2000NamespaceReplace, ImageHelper.Jpeg2000Namespace);

                        if (!JP2HelperBase.IsAudioOrVideo(k.File) && !String.IsNullOrEmpty(ImageHelper.ImageFilenameAppend) && !k.File.EndsWith(ImageHelper.ImageFilenameAppend))
                            k.File = string.Format("{0}{1}", k.File, ImageHelper.ImageFilenameAppend);

                        if (k.File.ToLower().Contains(ImageHelper.Jpeg2000Namespace.ToLower())) // file is a jpeg2000 image
                        {
                            var md_str = _IJP2Helper.GetJpeg2000Metadata(k.File, false);
                            md_str = md_str.Replace(@"\", @"\\");
                            var md = ser.Deserialize<ImageMetadata>(md_str);
                            k.ImageMetadata = md;
                        }
                        else
                        {
                            var file = k.File;

                            // most importantly we need the width and height of non-jpeg2000 images so can re-size them to thumbnails whilst retaining the aspect ratio
                            var d = ImageHelper.GetImageDimensions(file);

                            if ((file != null) && (file.Contains("'"))) // apostrophes in filenames, good at breaking JSON
                                file = file.Replace("'", @"\'");

                            k.ImageMetadata = new ImageMetadata { Identifier = file, Imagefile = file, Width = d[0], Height = d[1], DwtLevels = 0, Levels = 0, CompositingLayerCount = 0 };
                            // k.ImageMetadata = ser.Deserialize<Jpeg2000Metadata>(String.Format("{{ 'identifier': '{0}', 'imagefile': '{0}', 'width': '{1}', 'height': '{2}', 'dwtLevels': '0', 'levels': '0', 'compositingLayerCount': '0' }}", file, d[0], d[1]));
                        }
                    }
                }
            }
            //}
            //else
            //    throw new Exception("SolrRespository.addImageMetaData(): base_uri parameter empty");
            // return r_list;
        }


        public static void SetLanguageData(ref SolrSearchResults solr_results, string lang_id)
        {
            // lang_id = lang_id?.Split(new char[]{ '-' }, 1, StringSplitOptions.RemoveEmptyEntries)[0];

            // propertyInfo.SetValue(ship, value, null);
            if (solr_results != null && solr_results.Results != null && solr_results.Results.Count() > 0)
            {
                foreach (IInqItem k in solr_results.Results)
                {
                    // get any multilingual properties, select the relevant language array from their dictionary, copy to the
                    // normal version of the property, eg _mlKeywords -> Keywords. this way we filter out language data we don't
                    // need to send to the client.
                    Type t = k.GetType();
                    IList<PropertyInfo> mlprops = new List<PropertyInfo>(t.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance).Where(p => p.Name.StartsWith(MultilingualPropertyPrefix)).ToList());

                    foreach (PropertyInfo mlprop in mlprops)
                    {
                        var lang_dict = mlprop.GetValue(k, null) as Dictionary<string, List<string>>;

                        if (lang_dict != null)
                        {
                            var propinfo = k.GetType().GetProperty(mlprop.Name.Replace("_ml", ""));

                            if (propinfo != null && propinfo.PropertyType == typeof(List<string>))
                            {
                                // if we don't have a lang_id for some reason then just try to return some data (rather than nothing)
                                if (string.IsNullOrEmpty(lang_id))
                                    lang_id = lang_dict.Keys.FirstOrDefault(); // ?.Split(new char[] { '-' }, 1, StringSplitOptions.RemoveEmptyEntries)[0];

                                if (!string.IsNullOrEmpty(lang_id))
                                {
                                    if (lang_dict.ContainsKey(lang_id))
                                        propinfo.SetValue(k, lang_dict[lang_id], null);
                                }
                            }
                        }
                    }
                }
            }
        }


        public static SolrSearchResults GetSolrRecord(string id, IRepository _IRepository, IJP2Helper _IJP2Helper)
        {
            var results = _IRepository.GetRecord(id);
            ForceHttps(ref results);
            AddImageMetaData(ref results, _IJP2Helper);
            return results;
        }

        public static SolrSearchResults SearchSolr(SearchQuery sq, IRepository _IRepository, IJP2Helper _IJP2Helper)
        {
            // for properties which are multi-lingual and also facetted we store the languages in different Solr fields
            // eg InqItemRKD, property "Category" has an entry for nl and en languages, we store in separate Solr fields.
            // here we can select the correct Solr facetted field if a language has been selected which isn't the default 
            // eg in the web.config we have "category_nl|Category" but maybe the user has selected "en" via the UI
            // so we need to check for this here and update Facets as required

            var facets_lang_applied = new List<KeyValuePair<string, string>>();

            if (!string.IsNullOrEmpty(sq.LanguageID))
            {
                foreach (var f in Facets)
                {
                    var f_lang = f.Key;

                    var f_split = f.Key.Split(new[] { "_" }, StringSplitOptions.RemoveEmptyEntries);
                    if (f_split.Length > 1)
                    {
                        var lang_split = sq.LanguageID.Split(new[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                        if (f_split[1].ToLower().CompareTo(lang_split[0].ToLower()) != 0)
                            f_lang = string.Format("{0}_{1}", f_split[0], lang_split[0]);
                    }

                    facets_lang_applied.Add(new KeyValuePair<string, string>(f_lang, f.Value));
                }
            }

            var results = _IRepository.Search(sq, facets_lang_applied, FacetRanges);
            ForceHttps(ref results);
            AddImageMetaData(ref results, _IJP2Helper);
            SetLanguageData(ref results, sq.LanguageID);
            return results;
        }


        public static List<IInqItem> GetItemsFromIDStringList(string id_str, bool inc_children, IRepository _IRepository, IJP2Helper _IJP2Helper)
        {
            // id_str = id_str.ToUpper(); needed for guids and when using sql server as data source. solr stores as lower case, sql server as upper
            string[] ids = id_str.Split(new[] { "^" }, StringSplitOptions.RemoveEmptyEntries);
            var id_list = new List<string>(ids);
            return getItemAndChildren(id_list, inc_children, _IRepository, _IJP2Helper);
        }

        private static List<IInqItem> getItemAndChildren(ICollection<string> id_list, bool inc_children, IRepository _IRepository, IJP2Helper _IJP2Helper)
        {
            List<IInqItem> results = new List<IInqItem>();
            foreach (string id in id_list)
            {
                var res = _IRepository.GetRecord(id);
                SolrHelper.AddImageMetaData(ref res, _IJP2Helper);

                if (res.Results.Count == 1)
                {
                    var r0 = res.Results[0];
                    results.Add(r0); // add the parent result

                    // get the children
                    if (inc_children && (r0.ChildNodes != null) && (r0.ChildNodes.Count > 0))
                        results.AddRange(getItemAndChildren(new List<string>(r0.ChildNodes), inc_children, _IRepository, _IJP2Helper));
                }
            }
            return results;
        }


    }
}