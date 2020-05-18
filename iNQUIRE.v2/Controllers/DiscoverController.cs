//#define XML
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using iNQUIRE.Helper;
using iNQUIRE.Helpers;
using iNQUIRE.Models;
using iNQUIRE.ViewModels;
using Ninject.Modules;
using System.Linq;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using System.Net;
using System.Xml.Linq;
using System.Web.Script.Serialization;
using Facebook;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Reflection;
using Newtonsoft.Json.Linq;
using System.Text;

namespace iNQUIRE.Controllers
{
    public enum DownloadFormat { XML, RIS }

    public class DiscoverController : Controller
    {
        public static string MultilingualPropertyPrefix = "_ml"; 

        public static Guid ApplicationIdAspNet { get; set; }
        public static Guid ApplicationIdInquire { get; set; }
        public static string ExportFilename { get; set; }
        public static int ExportImageWidth { get; set; }
        public static int ExportImageHeight { get; set; }
        public static int ExportMaxImages { get; set; }
        // public static string ViewItemUri { get; set; }
        public static int SavedSearchesDisplayMax { get; set; }
        public static int TouchDoubleClickDelayMs { get; set; }
        public static string OpenDeepZoomTouchIcon { get; set; }
        public static Boolean AlwaysShowOpenDeepZoomTouchIcon { get; set; }
        public static string FacebookShareHashtag { get; set; }

        // public static bool DebugJp2HandlerRequests { get; set; }
        public static string SearchDebugParameters { get; set; }
        public static string SolrDebugParameters { get; set; }
        public static string IIPDebugParameters { get; set; }
        public static string DeepZoomDebugParameters { get; set; }

        private readonly Helper.IJP2Helper _IJP2Helper;
        private readonly IRepository _IRepository;
        private readonly IUserCollectionRepository<Workspace, WorkspaceItem, string> _IUserCollectionRepository;
        private readonly IUserTagRepository<Tag, TaggedItem, string> _IUserTagRepository;
        private readonly IUserNoteRepository<Note, string> _IUserNoteRepository;
        private readonly IUserSearchRepository _IUserSearchRepository;

        /// <summary>
        /// Application DB context
        /// </summary>
        protected ApplicationDbContext _ApplicationDbContext { get; set; }

        /// <summary>
        /// User manager - attached to application DB context
        /// </summary>
        protected UserManager<ApplicationUser> _UserManager { get; set; }

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

        private string MediaDirectoryFullUri
        {
            get { return String.Format("http://{0}{1}", Request.Url.Host, Url.Content(String.Format("~/{0}", _IJP2Helper.MediaDirectory))); }
        }

        private string ImageNotFound
        {
            get { return String.Format("http://{0}{1}", Request.Url.Host, Url.Content("~/Content/images/export_image_not_found.gif")); }
        }

        static DiscoverController()
        {
            Facets = new List<KeyValuePair<string, string>>();
            FacetRanges = new List<FacetRange>();
        }


        public DiscoverController(Helper.IJP2Helper ijp2helper, IRepository irepository,
            IUserCollectionRepository<Workspace, WorkspaceItem, string> iuser_collection,
            IUserTagRepository<Tag, TaggedItem, string> iuser_tag_repository,
            IUserNoteRepository<Note, string> iuser_note_repository,
            IUserSearchRepository iuser_search_repository)
        {
            _IJP2Helper = ijp2helper;
            _IRepository = irepository;
            _IUserCollectionRepository = iuser_collection;
            _IUserTagRepository = iuser_tag_repository;
            _IUserNoteRepository = iuser_note_repository;
            _IUserSearchRepository = iuser_search_repository;

            _ApplicationDbContext = new ApplicationDbContext();
            _UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_ApplicationDbContext));
        }

        public ActionResult Index(string id)
        {
            if (!String.IsNullOrEmpty(id))
                return RedirectToAction("Search", "Discover", new { term = String.Format("p=c+1,t+,rsrs+0,rsps+10,fa+,so+score%5Edesc,scids+,pid+,vi+{0}", id) });

            return View();
        }

        public ActionResult SocialMedia()
        {
            return View();
        }

        public ActionResult Test()
        {
            if (JP2ConfigHelper.DebugJp2HandlerRequests)
            {
                ViewBag.ViewerUrl = JP2ConfigHelper.ViewerUri;
                ViewBag.ResolverUrl = JP2ConfigHelper.ResolverUri;
                ViewBag.ReverseProxyUrl = string.Format("{0}{1}", JP2ConfigHelper.ApplicationBaseUri, JP2ConfigHelper.ProxyResolverFile);
                ViewBag.DeepZoomReverseProxyUrl = string.Format("{0}{1}", JP2ConfigHelper.ApplicationBaseUri, JP2ConfigHelper.DeepZoomViewerFile);
                ViewBag.SearchUrl = string.Format("{0}Discover/SearchAjax", JP2ConfigHelper.ApplicationBaseUri);
                ViewBag.SolrUrl = System.Configuration.ConfigurationManager.AppSettings["SolrUri"];
                ViewBag.SearchDebugParameters = SearchDebugParameters;
                ViewBag.SolrDebugParameters = SolrDebugParameters;
                ViewBag.IIPDebugParameters = IIPDebugParameters;
                ViewBag.DeepZoomDebugParameters = DeepZoomDebugParameters;
            }
            return View();
        }

        public ActionResult EmailExportAjax(string email_to, string message, string id_str, string lang_id)
        {
            // setResolvers();

            if (email_to == null)
                return Json("No destination email address", JsonRequestBehavior.AllowGet);

            if (id_str == null)
                return Json("No items selected", JsonRequestBehavior.AllowGet);

            var results = getItemsFromIDStringList(id_str, false);

            var export_items = new List<ExportItem>();
            var count = 0;
            foreach (IInqItem r in results)
            {
                var img_src = GetImageUri(r, ExportImageWidth, ExportImageHeight);
                // var img_src = _IJP2Helper.GetImageUri(r.ImageMetadata, MediaDirectoryFullUri, _IJP2Helper.ResolverUri, ExportImageWidth, ExportImageHeight);
                export_items.Add(new ExportItem(r, img_src, lang_id));
                count++;

                if (count >= ExportMaxImages)
                    break;
            }

            var export = new EmailExport(email_to, message, export_items, ImageNotFound);
            var queue = HttpContext.Application["email_queue"] as Queue<EmailExport>;
            queue.Enqueue(export);

            //var res = _IRepository.GetRecord(_IRepository.GetBaseUri(Request, Url), id);
            //var results_vm = new SearchAjaxViewModel(res) { Rows = 1, RowStart = 0 };

            return Json("ok", JsonRequestBehavior.AllowGet);
        }

        private List<DownloadFormat> getFormatsFromStringList(string str)
        {
            string[] formats = str.Split(new[] { "^" }, StringSplitOptions.RemoveEmptyEntries);
            var formats_enum = new List<DownloadFormat>();

            foreach (string s in formats)
            {
                if (s.ToLower().CompareTo("ris") == 0)
                    formats_enum.Add(DownloadFormat.RIS);
                else if (s.ToLower().CompareTo("xml") == 0)
                    formats_enum.Add(DownloadFormat.XML);
            }
            return formats_enum;
        }

        private List<IInqItem> getItemsFromIDStringList(string id_str, bool inc_children)
        {
            // id_str = id_str.ToUpper(); needed for guids and when using sql server as data source. solr stores as lower case, sql server as upper
            string[] ids = id_str.Split(new[] { "^" }, StringSplitOptions.RemoveEmptyEntries);
            var id_list = new List<string>(ids);
            return getItemAndChildren(id_list, inc_children);
        }

        private List<IInqItem> getItemAndChildren(ICollection<string> id_list, bool inc_children)
        {
            List<IInqItem> results = new List<IInqItem>();
            foreach (string id in id_list)
            {
                var res = _IRepository.GetRecord(id);
                addImageMetaData(ref res);

                if (res.Results.Count == 1)
                {
                    var r0 = res.Results[0];
                    results.Add(r0); // add the parent result

                    // get the children
                    if (inc_children && (r0.ChildNodes != null) && (r0.ChildNodes.Count > 0))
                        results.AddRange(getItemAndChildren(new List<string>(r0.ChildNodes), inc_children));
                }
            }
            return results;
        }

        public ActionResult Download(string id_str, string lang_id, string formats_str = "xml", bool inc_asset = true)
        {
            // setResolvers();

            if (id_str == null)
                return RedirectToAction("Search");

            var formats = getFormatsFromStringList(formats_str);
            var results = getItemsFromIDStringList(id_str, true);

            Response.ContentType = "application/zip";
            // If the browser is receiving a mangled zipfile, IIS Compression may cause this problem. Some members have found that
            //    Response.ContentType = "application/octet-stream"     has solved this. May be specific to Internet Explorer.

            Response.AppendHeader("content-disposition", "attachment; filename=\"Download.zip\"");
            Response.CacheControl = "Private";
            Response.Cache.SetExpires(DateTime.Now.AddMinutes(3)); // or put a timestamp in the filename in the content-disposition

            using (ZipOutputStream zipOutputStream = new ZipOutputStream(Response.OutputStream))
            {
                zipOutputStream.SetLevel(3); //0-9, 9 being the highest level of compression

                var ris_fn = String.Format("{0}.ris", ExportFilename);
                var xml_fn = String.Format("{0}.xml", ExportFilename);
                var xslt_fn = String.Format("{0}.xsl", ExportFilename);

                #region metadata

                if (formats.Contains(DownloadFormat.RIS))
                {
                    using (var ris_mem = new MemoryStream())
                    using (var writer = new StreamWriter(ris_mem))
                    {
                        foreach (IInqItem inq in results)
                            writer.Write(inq.ExportRis(lang_id));

                        writer.Flush();
                        ris_mem.Position = 0;
                        ZipHelper.ZipAdd(Response, zipOutputStream, ris_fn, ris_mem);
                    }
                }

                if (formats.Contains(DownloadFormat.XML))
                {
                    var xml = new XElement("items");
                    foreach (IInqItem inq in results)
                        xml.Add(inq.ExportXml(lang_id));

                    var xdoc = new XDocument(new XProcessingInstruction("xml-stylesheet", String.Format("type='text/xsl' href='{0}'", xslt_fn)), xml);
                    using (Stream xmls = new MemoryStream())
                    {
                        xdoc.Save(xmls);
                        xmls.Position = 0;
                        ZipHelper.ZipAdd(Response, zipOutputStream, xml_fn, xmls);
                    }

                    #region add xslt file
                    var xslt_ok = "";

                    try
                    {
                        var xslt_uri = String.Format("{0}{1}", _IJP2Helper.ApplicationBaseUri, xslt_fn); // Request.Url.Host, Url.Content(String.Format("~/{0}", xslt_filename))
                        using (Stream xslt = WebRequest.Create(xslt_uri).GetResponse().GetResponseStream())
                        {
                            ZipHelper.ZipAdd(Response, zipOutputStream, xslt_fn, xslt);
                        }
                    }
                    catch (Exception e)
                    {
                        // try getting the xslt via direct file IO request
                        try
                        {
                            var xslt_file = String.Format("{0}{1}", Request.PhysicalApplicationPath, xslt_fn);
                            if (System.IO.File.Exists(xslt_file))
                            {
                                using (Stream xslt = System.IO.File.OpenRead(xslt_file))
                                {
                                    ZipHelper.ZipAdd(Response, zipOutputStream, xslt_fn, xslt);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            xslt_ok = String.Format("Xslt web request failed: {0}, file request failed: {1}", e.Message, ex.Message);
                        }
                    }

                    if (!String.IsNullOrEmpty(xslt_ok))
                        Helper.LogHelper.StatsLog(null, "DiscoverController.Download() XSLT inclusion error: ", xslt_ok, null, null);
                    #endregion

                }

                #endregion

                #region images
                if (inc_asset)
                {
                    var count = 0;

                    foreach (IInqItem r in results)
                    {
                        string img_src = GetImageUri(r, ExportImageWidth, ExportImageHeight);

                        // images
                        using (Stream fs = ImageHelper.GetImageStream(img_src, ImageNotFound))
                        {
                            ZipHelper.ZipAdd(Response, zipOutputStream, String.Format("{0}.jpg", r.ID), fs);
                        }
                        count++;

                        if (count >= ExportMaxImages)
                            break;
                    }
                }
                #endregion

                zipOutputStream.Close();
                Response.Flush();
                Response.End();
                return File(zipOutputStream, "application/zip");
            }
        }

        public string GetImageUri(IInqItem r, int max_width, int max_height)
        {
            string img_src;
            var r_iiif = r as InqItemIIIFBase;

            if (r_iiif == null)
                img_src = _IJP2Helper.GetImageUri(r.ImageMetadata, MediaDirectoryFullUri, _IJP2Helper.ResolverUri, max_width, max_height);
            else
                img_src = r_iiif.GetImageUri(max_width, max_height);

            return img_src;
        }

        public void SetupSearchViewBag(string id)
        {
            ViewBag.ViewItemID = id;

            ApplicationUser user = _UserManager.FindById(User.Identity.GetUserId());

            ViewBag.UserID = User.Identity.GetUserId();
            ViewBag.UserEmail = (user != null) ? user.Email : "";

            if (!string.IsNullOrEmpty(id))
            {
                var res = GetSolrRecord(id);

                if (res.Results.Count == 1)
                {
                    var r = res.Results[0] as InqItemBase;

                    if (r != null)
                    {
                        var fb_img_w = 1200;
                        var fb_img_h = 650;

                        ViewBag.ogUrl = Request.Url.OriginalString; //string.Format("{0}://{1}{2}", "https", Request.Url.Authority, Url.Content("~"));
                        ViewBag.ogType = "website";
                        ViewBag.ogTitle = r.Title;

                        ViewBag.ogDescription = r.Description;
                        ViewBag.ogImage = GetImageUri(r, fb_img_w, fb_img_h);

                        var preview_img_w = 0;
                        var preview_img_h = 0;

                        if (r.AspectRatio > 1)
                        {
                            preview_img_w = fb_img_w;
                            preview_img_h = Convert.ToInt32(fb_img_w / r.AspectRatio);
                        }
                        else
                        {
                            preview_img_h = fb_img_h;
                            preview_img_w = Convert.ToInt32(fb_img_h * r.AspectRatio);
                        }

                        ViewBag.ogImageWidth = preview_img_w;
                        ViewBag.ogImageHeight = preview_img_h;

                        ViewBag.Title = r.Title;
                    }
                }
            }

            ViewBag.FacebookShareHashtag = FacebookShareHashtag;
            ViewBag.JP2Resolver = _IJP2Helper.ResolverReverseProxy;
            ViewBag.ZoomViewerHeightPx = _IJP2Helper.ZoomViewerHeightPx;
            ViewBag.MediaDirectory = _IJP2Helper.MediaDirectory;
            ViewBag.Jpeg2000Namespace = ImageHelper.Jpeg2000Namespace;
            ViewBag.Jpeg2000NamespaceReplace = ImageHelper.Jpeg2000NamespaceReplace;
            ViewBag.SortFieldsString = SortFieldsString;
            ViewBag.ColonInFieldNames = SolrHelper.ColonInFieldNames;
            ViewBag.ConcatFields = SolrHelper.ConcatenateFields;
            ViewBag.ConcatSeparator = SolrHelper.FieldConcatenationString;
            ViewBag.ViewItemBaseUri = InqItemBase.ViewItemBaseUri;
            ViewBag.SavedSearchesDisplayMax = SavedSearchesDisplayMax;
            ViewBag.DeepZoomViewer = _IJP2Helper.DeepZoomViewerReverseProxy;
            ViewBag.TouchDoubleClickDelayMs = TouchDoubleClickDelayMs;
            ViewBag.OpenDeepZoomTouchIcon = OpenDeepZoomTouchIcon;
            ViewBag.AlwaysShowOpenDeepZoomTouchIcon = AlwaysShowOpenDeepZoomTouchIcon;
        }

        public ActionResult ViewItem(string id)
        {
            SetupSearchViewBag(id);
            return View("Search");
        }

        public ActionResult Search()
        {
            // get user_id from db as not included in the User object
            SetupSearchViewBag("");
            return View();
        }

        public ActionResult DeepZoom(string id, int h, int w, int img_w, int img_h)
        {
            // redundant, for use when launching openseadragon (or old seajax viewer) in an <iframe>, kept in case support for IE9 is needed (as openseadragon buttons don't work too good with IE9)

            //if (_deepZoomViewer == null)
            //    _deepZoomViewer = String.Format("{0}{1}", _IRepository.GetBaseUri(Request, Url), DeepZoomViewerFile);

            ViewBag.DeepZoomViewer = _IJP2Helper.DeepZoomViewerReverseProxy; //_deepZoomViewer;
            ViewBag.DeepZoomQueryParameter = _IJP2Helper.DeepZoomQueryParameter;
            ViewBag.DeepZoomID = id;
            ViewBag.Height = h;
            ViewBag.Width = w;
            ViewBag.ImageHeight = img_h;
            ViewBag.ImageWidth = img_w;

            return View();
        }

        public ActionResult Print(string id, double? w, double? h)
        {
            // setResolvers();

            if (w == null)
                w = 600;

            if (h == null)
                h = 600;

            if (String.IsNullOrEmpty(id))
                id = "48A69DEE-6A29-491D-8E2C-BE33D2A9B63B"; // return RedirectToAction("Search");

            var res = _IRepository.GetRecord(id);
            addImageMetaData(ref res);

            if ((res == null) || (res.Results == null) || (res.Results.Count != 1))
                return RedirectToAction("Search");

            var r = res.Results[0];

            ViewBag.ImageUri = GetImageUri(r, (int)w, (int)h);//  _IJP2Helper.GetImageUri(r.ImageMetadata, MediaDirectoryFullUri, _IJP2Helper.ResolverReverseProxy, (double)w, (double)h);
            return View(r);
        }

        #region searches
        //[HttpPost]
        public ActionResult SearchAjax(string user_id, string term, string collection_ids, int rows, int row_start, string parent_id, string sort_orders, string facet_constraints, string lang_id) // SearchFacetViewModel facet)
        {
            var sq = new SearchQuery(user_id, term, collection_ids, rows, row_start, parent_id, sort_orders, facet_constraints, lang_id);

            if (String.IsNullOrEmpty(user_id))
                user_id = Guid.Empty.ToString();

            //XmlDataHelper.SearchXml(Request.GetBaseUri(Url), term, new List<string>(ids), collection_search)
            var results = SearchSolr(sq);

            // save all searches, even if user not logged in (for complete stats). if start row == 0 assume new search (and not a seach page nav click)
            var kvp = new KeyValuePair<Guid, String>(Guid.Empty, null);
            if (row_start == 0)
                kvp = _IUserSearchRepository.SearchSave(ApplicationIdInquire, lang_id, user_id, sq, results.NumFound);

            results.SearchID = kvp.Key; // if search was saved this will NOT be Guid.Empty, so then add it to users list as a newly saved search
            sq.SearchID = kvp.Key;

            results.SearchDisplayName = kvp.Value;
            sq.DisplayName = kvp.Value;

            sq.NumFound = results.NumFound;
            results.SearchQuery = sq;

            var results_vm = new SearchAjaxViewModel(results) { Rows = rows, RowStart = row_start };
            return Json(results_vm, JsonRequestBehavior.AllowGet);
        }

        private SolrSearchResults GetSolrRecord(string id)
        {
            var results = _IRepository.GetRecord(id);
            forceHttps(ref results);
            addImageMetaData(ref results);
            return results;
        }

        private SolrSearchResults SearchSolr(SearchQuery sq)
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
            forceHttps(ref results);
            addImageMetaData(ref results);
            setLanguageData(ref results, sq.LanguageID);
            return results;
        }

        protected void forceHttps(ref SolrSearchResults solr_results)
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

        protected void addImageMetaData(ref SolrSearchResults solr_results)
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


        protected void setLanguageData(ref SolrSearchResults solr_results, string lang_id)
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


        public ActionResult SearchDeleteAjax(string user_id, string search_id)
        {
            if ((String.IsNullOrEmpty(user_id)) || (String.IsNullOrEmpty(search_id)))
                return null;

            var r = _IUserSearchRepository.SearchDelete(user_id, new Guid(search_id));
            return Json(r, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSearchesForUserAjax(string user_id)
        {
            if (String.IsNullOrEmpty(user_id))
                return null;

            var r = _IUserSearchRepository.GetSearches(ApplicationIdInquire, user_id, SavedSearchesDisplayMax);
            return Json(r, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSearchSuggestionsAjax(string lang_id, string str)
        {
            if (String.IsNullOrEmpty(str))
                return null;

            var res = _IRepository.GetSearchSuggestions(lang_id, str);
            var sug = new List<string>();

            foreach (SolrNet.Impl.SpellCheckResult s in res.SpellChecking)
                sug.AddRange(s.Suggestions);

            return Json(sug, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetRecordAjax(string id, string lang_id)
        {
            if (String.IsNullOrEmpty(id))
                return null;

            var res = _IRepository.GetRecord(id);
            forceHttps(ref res);
            addImageMetaData(ref res);
            setLanguageData(ref res, lang_id);
            var results_vm = new SearchAjaxViewModel(res) { Rows = 1, RowStart = 0 };
            return Json(results_vm, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region facebook
        // could store it here but not sure we really need to, as can be re-requested via JS methods easily, and search page doesn't reload anyway so shouldn't lose it..?
        /*public ActionResult fbLoginAjax(string access_token)
        {
            Session["fbAccessToken"] = access_token;
            return Json(access_token, JsonRequestBehavior.AllowGet);
        }*/

        // This type of direct posting is deprecated from the Facebook API 1 Aug 2018,
        // now have to use Javascript share method (only shares a link, not post a photo)
        //public ActionResult fbUploadPhotoAjax(string id_str, string access_token)
        //{
        //    string r = "?";
        //    var results = getItemsFromIDStringList(id_str, false);
        //    if (results.Count == 1)
        //    {
        //        var result = results[0];
        //        var client = new FacebookClient(access_token);
        //        //Create a new dictionary of objects, with string keys
        //        Dictionary<string, object> parameters = new Dictionary<string, object>();
        //        var ImageName = String.Format("{0}.jpg", result.Title);
        //        var ImagePath = String.Format(@"F:\Shared Documents\Test Images\{0}", ImageName);

        //        string strDescription = result.Title;

        //        //Add elements to the dictionary
        //        if (string.IsNullOrEmpty(ImagePath) == false)
        //        {
        //            //There is an Image to add to the parameters                
        //            var media = new FacebookMediaObject
        //            {
        //                FileName = ImageName,
        //                ContentType = "image/jpeg"
        //            };

        //            string img_src = null;

        //            var iiif_result = result as InqItemIIIFBase;

        //            if (iiif_result != null)
        //                img_src = iiif_result.GetImageUri(ExportImageWidth, ExportImageHeight);
        //            else
        //                img_src = _IJP2Helper.GetImageUri(result.ImageMetadata, MediaDirectoryFullUri, _IJP2Helper.ResolverUri, ExportImageWidth, ExportImageHeight);

        //            media.SetValue(ImageHelper.GetImageBytes(img_src, ImageNotFound));

        //            parameters.Add("source", media);
        //            parameters.Add("message", strDescription);

        //            try
        //            {
        //                //client.PostCompleted += fbPostCompleted;
        //                //dynamic result = client.PostTaskAsync("/me/photos", parameters);
        //                dynamic post_result = client.Post("/me/photos", parameters);
        //                r = "ok";
        //                Helper.LogHelper.StatsLog(null, "fbUploadPhotoAjax() [async] ", r, null, null);
        //            }
        //            catch (Exception ex)
        //            {
        //                r = ex.Message;
        //            }
        //        }
        //    }
        //    else
        //        r = "Error finding item";

        //    return Json(r, JsonRequestBehavior.AllowGet);
        //}

        /*void fbPostCompleted(object sender, FacebookApiEventArgs e)
        {
            string r;

            if (e.Error == null)
                r = "ok";
            else
                r = String.Format("failed: {0}", e.Error.Message);

            Helper.LogHelper.StatsLog(null, "fbUploadPhotoAjax() [async] ", r, null, null);
        }*/

        public ActionResult fbGetUser(string id)
        {
            var client = new Facebook.FacebookClient();
            dynamic me = client.Get(id);
            return Json(me, JsonRequestBehavior.AllowGet);
        }
        #endregion


        #region notes ajax methods
        public ActionResult NoteAddAjax(string lang_id, string user_id, string item_id, string text, bool public_note)
        {
            if (String.IsNullOrEmpty(item_id) || String.IsNullOrEmpty(user_id))
                return null;

            var r = _IUserNoteRepository.NoteAdd(ApplicationIdInquire, lang_id, user_id, item_id, text, public_note);
            return Json(r, JsonRequestBehavior.AllowGet);
        }

        public ActionResult NoteUpdateAjax(string note_id, string user_id, string text, bool public_note)
        {
            if (String.IsNullOrEmpty(note_id) || String.IsNullOrEmpty(user_id))
                return null;

            var r = _IUserNoteRepository.NoteUpdate(new Guid(note_id), user_id, text, public_note);
            return Json(r, JsonRequestBehavior.AllowGet);
        }

        public ActionResult NoteDeleteAjax(string note_id, string user_id)
        {
            if (String.IsNullOrEmpty(note_id) || String.IsNullOrEmpty(user_id))
                return null;

            var r = _IUserNoteRepository.NoteDelete(new Guid(note_id), user_id);
            return Json(r, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetNotesForItemAjax(string user_id, string item_id)
        {
            List<NoteInfo> r;

            if (String.IsNullOrEmpty(user_id) || String.IsNullOrEmpty(item_id)) // user_id will be empty if not logged in, so quite common
                r = new List<NoteInfo>();
            else
                r = _IUserNoteRepository.GetNotesForItem(ApplicationIdInquire, user_id, item_id);
            
            return Json(r, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetPublicNotesForItemAjax(string item_id, bool approved)
        {
            if (String.IsNullOrEmpty(item_id))
                return null;

            var r = _IUserNoteRepository.GetPublicNotesForItem(ApplicationIdInquire, item_id, approved);
            var j = Json(r, JsonRequestBehavior.AllowGet);
            return j;
        }
        #endregion

        #region tags ajax methods
        public ActionResult TagItemAjax(string lang_id, string user_id, string title, string tag_id, string item_id)
        {
            if (String.IsNullOrEmpty(lang_id) || String.IsNullOrEmpty(item_id) || String.IsNullOrEmpty(user_id))
                return null;

            Guid tag_guid;

            if (String.IsNullOrEmpty(tag_id))
                tag_guid = Guid.Empty;
            else
                tag_guid = new Guid(tag_id);

            var r = _IUserTagRepository.TagItem(ApplicationIdInquire, lang_id, user_id, title, tag_guid, item_id);
            return Json(r, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetItemTagsAjax(string lang_id, string item_id)
        {
            if (String.IsNullOrEmpty(lang_id) || String.IsNullOrEmpty(item_id))
                return null;

            var r = _IUserTagRepository.GetItemTags(ApplicationIdInquire, lang_id, item_id);
            return Json(r, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetUserItemTagsAjax(string lang_id, string item_id, string user_id)
        {
            List<TagInfo> r;

            if (String.IsNullOrEmpty(lang_id) || String.IsNullOrEmpty(item_id) || String.IsNullOrEmpty(user_id))
                r = new List<TagInfo>();
            else
                r = _IUserTagRepository.GetUserItemTags(ApplicationIdInquire, lang_id, item_id, user_id);
            
            return Json(r, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UnTagItemAjax(string user_id, string tag_id, string item_id)
        {
            if (String.IsNullOrEmpty(user_id) || String.IsNullOrEmpty(tag_id) || String.IsNullOrEmpty(item_id))
                return null;

            var r = _IUserTagRepository.UnTagItem(user_id, new Guid(tag_id), item_id);
            return Json(r, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region collection ajax methods
        public ActionResult CollectionNewAjax(string user_id, string title)
        {
            if ((!Request.IsAuthenticated) || (String.IsNullOrEmpty(user_id)))
                return null;

            var r = _IUserCollectionRepository.CollectionNew(ApplicationIdInquire, user_id, title);
            return Json(r, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CollectionDeleteAjax(string collection_id)
        {
            if (String.IsNullOrEmpty(collection_id))
                return null;

            var r = _IUserCollectionRepository.CollectionDelete(new Guid(collection_id));
            return Json(r, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CollectionRenameAjax(string collection_id, string new_title)
        {
            if (String.IsNullOrEmpty(collection_id))
                return null;

            var r = _IUserCollectionRepository.CollectionRename(new Guid(collection_id), new_title);
            return Json(r, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CollectionAddItemAjax(string collection_id, string item_id, string title, string search_term, string language_id, int position)
        {
            if (String.IsNullOrEmpty(collection_id) || String.IsNullOrEmpty(item_id))
                return null;

            var r = _IUserCollectionRepository.CollectionAddItem(new Guid(collection_id), item_id, title, search_term, language_id, position);
            return Json(r, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CollectionDeleteItemAjax(string collection_id, string item_id)
        {
            if (String.IsNullOrEmpty(collection_id) || String.IsNullOrEmpty(item_id))
                return null;

            var r = _IUserCollectionRepository.CollectionDeleteItem(new Guid(collection_id), item_id);
            return Json(r, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CollectionUpdateItemAjax(string collection_id, string item_id, string lang_id, string notes, int pos_x, int pos_y, int position, string keywords, string search_term)
        {
            if (String.IsNullOrEmpty(collection_id) || String.IsNullOrEmpty(item_id))
                return null;

            var r = _IUserCollectionRepository.CollectionUpdateItem(new Guid(collection_id), item_id, lang_id, notes, pos_x, pos_y, position, keywords, search_term);
            return Json(r, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CollectionUpdateItemPositionAjax(string collection_id, string item_id, int position)
        {
            if (String.IsNullOrEmpty(collection_id) || String.IsNullOrEmpty(item_id))
                return null;

            var r = _IUserCollectionRepository.CollectionUpdateItemPosition(new Guid(collection_id), item_id, position);
            return Json(r, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CollectionUpdateItemPositionXYAjax(string collection_id, string item_id, int pos_x, int pos_y)
        {
            if (String.IsNullOrEmpty(collection_id) || String.IsNullOrEmpty(item_id))
                return null;

            var r = _IUserCollectionRepository.CollectionUpdateItemPositionXY(new Guid(collection_id), item_id, pos_x, pos_y);
            return Json(r, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CollectionGetAjax(string collection_id)
        {
            if (String.IsNullOrEmpty(collection_id))
                return null;

            var r = _IUserCollectionRepository.CollectionGet(new Guid(collection_id));
            return Json(r, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CollectionGetItemsAjax(string collection_id)
        {
            if (String.IsNullOrEmpty(collection_id))
                return null;

            var r = _IUserCollectionRepository.CollectionGetItems(new Guid(collection_id));
            return Json(r, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CollectionsListUserAjax(string user_id)
        {
            if (String.IsNullOrEmpty(user_id))
                return null;

            var r = _IUserCollectionRepository.CollectionsUser(ApplicationIdInquire, user_id);
            return Json(r, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region GetJpeg2000Metadata() ajax json
        public ActionResult GetJpeg2000Metadata(string id)
        {
            id = Url.Encode(id);
            return Json(_IJP2Helper.GetJpeg2000Metadata(id, true), JsonRequestBehavior.AllowGet);
        }
        #endregion


        [HttpPost]
        public ActionResult CreateJpeg2000(string id)
        {
            var img_id = ImageHelper.CreateJpeg2000(id);
            var img_ok = (String.IsNullOrEmpty(img_id) == false);

            if (img_ok)
                _IRepository.UpdateFileFieldToJpeg2000(id, img_id); // need to also update sql server if arm db, need to override UpdateSolrFileFieldToJpeg2000 in ArmDbSolrRepository

            return Json(img_ok);
        }

        //public ActionResult GetAutoCompleteData()
        //{
        //    //XDocument imgsXml = XDocument.Load(uri);
        //    //Results = Xml Server.MapPath(@"~\App_Data\" + _appDataXml); // jpeg2000s.xml
        //    return Json(sb.ToString(), JsonRequestBehavior.AllowGet);
        //}


        public ActionResult TestDatabase()
        {
            var result = new JObject();

            try
            {
                using (var db = new iNQUIRELiteDataContext())
                {
                    var db_exists = db.DatabaseExists();
                    System.Data.Common.DbConnectionStringBuilder builder = new System.Data.Common.DbConnectionStringBuilder();
                    builder.ConnectionString = db.Connection.ConnectionString;
                    result.Add("Server", builder["Data Source"] as string);
                    result.Add("Database", builder["Initial Catalog"] as string);
                    result.Add("Result", db.DatabaseExists() ? "OK" : "Failed to connect");
                }
            }
            catch (Exception e)
            {
                result.Add("Error", e.Message);
            }

            return Content(result.ToString(), "application/json");
        }

        [HttpPost]
        public ActionResult TestUrl(string url)
        {
            HttpWebResponse response;
            var result = new JObject();

            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                response = (HttpWebResponse)request.GetResponse();
                result.Add("StatusCode", (int)response.StatusCode);
                result.Add("StatusDescription", response.StatusDescription);
            }
            catch (WebException ex)
            {
                //remote url not found, log an error and send 404 to client 
                var err_response = ex.Response != null ? (HttpWebResponse)ex.Response : null;
                var status_code = 404;
                var status_desc = "No response";

                if (err_response != null)
                {
                    status_code = (int)err_response.StatusCode;
                    status_desc = err_response.StatusDescription;
                }

                LogHelper.StatsLog(null, "DiscoverController.TestUrl()", String.Format("Failed, Response status code: {0} , Response status desc: {1}, WebExceptionMessage: {2}, Url: {3}", status_code, status_desc, ex.Message, url), null, null);

                result.Add("Error", ex.Message);
                result.Add("StatusCode", status_code);
                result.Add("StatusDescription", status_desc);
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            using (Stream receiveStream = response.GetResponseStream())
            {
                if (receiveStream != null)
                {
                    //if ((response.ContentType.ToLower().IndexOf("html") >= 0) ||
                    //    (response.ContentType.ToLower().IndexOf("javascript") >= 0))
                    //{
                        //this response is HTML Content, so we must parse it
                        using (var readStream = new StreamReader(receiveStream, Encoding.Default))
                        {
                            string content = readStream.ReadToEnd();// HandlerHelper.ParseHtmlResponse(readStream.ReadToEnd(), "");
                            result.Add("Data", content);
                        }
                    //}
                    //else
                    //{
                    //    //the response is not HTML Content

                    //    if (IIPImageIIS.IIPImageHandler.IsJpeg(remoteUrl))
                    //        context.Response.ContentType = "image/jpeg";

                    //    if (IsJson(remoteUrl))
                    //    {
                    //        context.Response.ContentType = "application/json";
                    //        context.Response.ContentEncoding = Encoding.UTF8;
                    //    }

                    //    var buff = new byte[1024];
                    //    int bytes;
                    //    while ((bytes = receiveStream.Read(buff, 0, 1024)) > 0)
                    //    {
                    //        //Write the stream directly to the client 
                    //        context.Response.OutputStream.Write(buff, 0, bytes);
                    //    }
                    //}
                }
            }

            response.Close();
            return Content(result.ToString(), "application/json");
        }


        public ActionResult GetLog(int month, int year)
        {
            return Json(LogHelper.GetLog(month, year), JsonRequestBehavior.AllowGet);
        }
    }

#if XML
    public class SolrRepositoryModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IRepository>().To<XmlRespository>();
            // Bind<IUserCollectionRepository>().To<LinqToSqlUserCollectionRepository>();
        }
    }
#else
    public class SolrRepositoryModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IRepository>().To<iNQUIRESolrRepository>();
            Bind(typeof(IUserCollectionRepository<,,>)).To(typeof(LinqToSqlUserCollectionRepository));
            Bind(typeof(IUserTagRepository<,,>)).To(typeof(LinqToSqlUserTagRepository));
            Bind(typeof(IUserNoteRepository<,>)).To(typeof(LinqToSqlUserNoteRepository));
            Bind(typeof(IUserSearchRepository)).To(typeof(LinqToSqlUserSearchRepository));
        }
    }
#endif

    public class JP2HelperModule : NinjectModule
    {
        public override void Load()
        {
            Bind<Helper.IJP2Helper>().To<Helper.IIPImageHelper>();
            // Bind<Helper.IJP2Helper>().To<Helper.DjatokaHelper>();
        }
    }
}