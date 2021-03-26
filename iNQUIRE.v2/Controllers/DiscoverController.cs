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
using Newtonsoft.Json;

namespace iNQUIRE.Controllers
{
    public enum DownloadFormat { XML, RIS }

    public class DiscoverController : Controller
    {
        public static Guid ApplicationIdAspNet { get; set; }
        public static Guid ApplicationIdInquire { get; set; }
        public static string ExportFilename { get; set; }
        // public static string ViewItemUri { get; set; }
        public static int SavedSearchesDisplayMax { get; set; }
        public static int TouchDoubleClickDelayMs { get; set; }
        public static string OpenDeepZoomTouchIcon { get; set; }
        public static Boolean AlwaysShowOpenDeepZoomTouchIcon { get; set; }
        public static string FacebookShareHashtag { get; set; }
        public static Boolean UseTimeSince { get; set; }

        // public static bool DebugJp2HandlerRequests { get; set; }
        public static string SearchDebugParameters { get; set; }
        public static string SolrDebugParameters { get; set; }
        public static string IIPDebugParameters { get; set; }
        public static string DeepZoomDebugParameters { get; set; }

        public static List<KeyValuePair<string,string>> Languages { get; set; }
        public static List<string> MultiLingualSolrFields { get; set; }

        private readonly Helper.IJP2Helper _IJP2Helper;
        private readonly IRepository _IRepository;
        // private readonly IUserCollectionRepository<Workspace, WorkspaceItem, string> _IUserCollectionRepository;
        // private readonly IUserTagRepository<Tag, TaggedItem, string> _IUserTagRepository;
        // private readonly IUserNoteRepository<Note, string> _IUserNoteRepository;
        // private readonly IUserSearchRepository _IUserSearchRepository;

        /// <summary>
        /// Application DB context
        /// </summary>
        protected ApplicationDbContext _ApplicationDbContext { get; set; }

        /// <summary>
        /// User manager - attached to application DB context
        /// </summary>
        protected UserManager<ApplicationUser> _UserManager { get; set; }


        static DiscoverController()
        {
        }


        public DiscoverController(IJP2Helper ijp2helper, IRepository irepository
            // IUserCollectionRepository<Workspace, WorkspaceItem, string> iuser_collection
            //IUserTagRepository<Tag, TaggedItem, string> iuser_tag_repository,
            //IUserNoteRepository<Note, string> iuser_note_repository,
            // IUserSearchRepository iuser_search_repository
            )
        {
            _IJP2Helper = ijp2helper;
            _IRepository = irepository;
            //_IUserCollectionRepository = iuser_collection;
            //_IUserTagRepository = iuser_tag_repository;
            //_IUserNoteRepository = iuser_note_repository;
            // _IUserSearchRepository = iuser_search_repository;

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
            else
                ViewBag.Error = "To fully enable this page set DebugJp2HandlerRequests to true in the web.config, remember to set back to false when testing is complete.";

            return View();
        }

        //public ActionResult EmailExportAjax(string email_to, string message, string id_str, string lang_id)
        //{
        //    // setResolvers();

        //    if (email_to == null)
        //        return Json("No destination email address", JsonRequestBehavior.AllowGet);

        //    if (id_str == null)
        //        return Json("No items selected", JsonRequestBehavior.AllowGet);

        //    var results = SolrHelper.GetItemsFromIDStringList(id_str, false, _IRepository, _IJP2Helper);

        //    var export_items = new List<ExportItem>();
        //    var count = 0;
        //    foreach (IInqItem r in results)
        //    {
        //        var img_src = ImageHelper.GetImageUri(r, ExportImageWidth, ExportImageHeight, Request.Url.Host, );
        //        // var img_src = _IJP2Helper.GetImageUri(r.ImageMetadata, MediaDirectoryFullUri, _IJP2Helper.ResolverUri, ExportImageWidth, ExportImageHeight);
        //        export_items.Add(new ExportItem(r, img_src, lang_id));
        //        count++;

        //        if (count >= ExportMaxImages)
        //            break;
        //    }

        //    var export = new EmailExport(email_to, message, export_items, ImageHelper.ImageNotFound(Request.Url.Host, Url.Content("~/Content/images/export-image-not-found.png")));
        //    var queue = HttpContext.Application["email_queue"] as Queue<EmailExport>;
        //    queue.Enqueue(export);

        //    //var res = _IRepository.GetRecord(_IRepository.GetBaseUri(Request, Url), id);
        //    //var results_vm = new SearchAjaxViewModel(res) { Rows = 1, RowStart = 0 };

        //    return Json("ok", JsonRequestBehavior.AllowGet);
        //}

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

        public ActionResult Download(string id_str, string lang_id, string formats_str = "xml", bool inc_asset = true)
        {
            // setResolvers();

            if (id_str == null)
                return RedirectToAction("Search");

            var formats = getFormatsFromStringList(formats_str);
            var results = SolrHelper.GetItemsFromIDStringList(id_str, true, _IRepository, _IJP2Helper);

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
                        string img_src = ImageHelper.GetImageUri(r, ImageHelper.ExportImageWidth, ImageHelper.ExportImageHeight, Request.Url.GetLeftPart(UriPartial.Authority), _IJP2Helper.MediaDirectory, _IJP2Helper);

                        // images
                        using (Stream fs = ImageHelper.GetImageStream(img_src, Url.Content("~/Content/images/export-image-not-found.png")))
                        {
                            ZipHelper.ZipAdd(Response, zipOutputStream, String.Format("{0}.jpg", r.ID), fs);
                        }
                        count++;

                        if (count >= ImageHelper.ExportMaxImages)
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

        public void SetupSearchViewBag(string id)
        {
            ViewBag.ViewItemID = id;

            ApplicationUser user = _UserManager.FindById(User.Identity.GetUserId());

            ViewBag.UserID = User.Identity.GetUserId();
            ViewBag.UserEmail = (user != null) ? user.Email : "";

            if (!string.IsNullOrEmpty(id))
            {
                var res = SolrHelper.GetSolrRecord(id, _IRepository, _IJP2Helper);

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
                        ViewBag.ogImage = ImageHelper.GetImageUri(r, fb_img_w, fb_img_h, Request.Url.GetLeftPart(UriPartial.Authority), _IJP2Helper.MediaDirectory, _IJP2Helper);

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
            ViewBag.SortFieldsString = SolrHelper.SortFieldsString;
            ViewBag.ColonInFieldNames = SolrHelper.ColonInFieldNames;
            ViewBag.ConcatFields = SolrHelper.ConcatenateFields;
            ViewBag.ConcatSeparator = SolrHelper.FieldConcatenationString;
            ViewBag.ViewItemBaseUri = InqItemBase.ViewItemBaseUri;
            ViewBag.SavedSearchesDisplayMax = SavedSearchesDisplayMax;
            ViewBag.DeepZoomViewer = _IJP2Helper.DeepZoomViewerReverseProxy;
            ViewBag.TouchDoubleClickDelayMs = TouchDoubleClickDelayMs;
            ViewBag.OpenDeepZoomTouchIcon = OpenDeepZoomTouchIcon;
            ViewBag.AlwaysShowOpenDeepZoomTouchIcon = AlwaysShowOpenDeepZoomTouchIcon;
            ViewBag.UseTimeSince = UseTimeSince;

            ViewBag.Languages = JsonConvert.SerializeObject(Languages.Select(x => new { code = x.Key, name = x.Value }));
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
            SolrHelper.AddImageMetaData(ref res, _IJP2Helper);

            if ((res == null) || (res.Results == null) || (res.Results.Count != 1))
                return RedirectToAction("Search");

            var r = res.Results[0];

            ViewBag.ImageUri = ImageHelper.GetImageUri(r, (int)w, (int)h, Request.Url.GetLeftPart(UriPartial.Authority), _IJP2Helper.MediaDirectory, _IJP2Helper); //  _IJP2Helper.GetImageUri(r.ImageMetadata, MediaDirectoryFullUri, _IJP2Helper.ResolverReverseProxy, (double)w, (double)h);
            return View(r);
        }


       //private string JP2MediaDir
       // {
       //     get { return Url.Content(String.Format("~/{0}", _IJP2Helper.MediaDirectory)); }
       // }
        #region moved to ImageHelper.cs
        //private string MediaDirectoryFullUri
        //{
        //    get { return String.Format("http://{0}{1}", Request.Url.Host, Url.Content(String.Format("~/{0}", _IJP2Helper.MediaDirectory))); }
        //}

        //private string ImageNotFound
        //{
        //    get { return String.Format("http://{0}{1}", Request.Url.Host, Url.Content("~/Content/images/export-image-not-found.png")); }
        //}

        //public string GetImageUri(IInqItem r, int max_width, int max_height)
        //{
        //    string img_src;
        //    var r_iiif = r as InqItemIIIFBase;

        //    if (r_iiif == null)
        //        img_src = _IJP2Helper.GetImageUri(r.ImageMetadata, MediaDirectoryFullUri, _IJP2Helper.ResolverUri, max_width, max_height);
        //    else
        //        img_src = r_iiif.GetImageUri(max_width, max_height);

        //    return img_src;
        //}
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


        public ActionResult ReloadSolrCore()
        {
            // Just use a simple http request.
            // Ideally would make this a method of SolrRepository but I think currently used version of SolrNet (0.4) doesn't support
            // the core Admin and Reload methods? Could update in the future and try again.

            // SolrUrl: http://inquire.westeurope.cloudapp.azure.com:8983/solr/core_rkd_1
            // Need to generate: http://inquire.westeurope.cloudapp.azure.com:8983/solr/admin/cores?action=RELOAD&core=core_rkd_1

            HttpWebResponse response;
            var result = new JObject();
            string url = null;

            try
            {
                var solr_url = System.Configuration.ConfigurationManager.AppSettings["SolrUri"];
                var last_slash_pos = solr_url.LastIndexOf("/");
                var core_name = solr_url.Substring(last_slash_pos + 1);
                var solr_url_base = solr_url.Substring(0, last_slash_pos + 1);
                url = string.Format("{0}admin/cores?action=RELOAD&core={1}", solr_url_base, core_name);
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

                LogHelper.StatsLog(null, "DiscoverController.ReloadSolrCore()", String.Format("Failed, Response status code: {0} , Response status desc: {1}, WebExceptionMessage: {2}, Url: {3}", status_code, status_desc, ex.Message, url), null, null);

                result.Add("Error", ex.Message);
                result.Add("StatusCode", status_code);
                result.Add("StatusDescription", status_desc);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                result.Add("Error", e.Message);
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            using (Stream receiveStream = response.GetResponseStream())
            {
                if (receiveStream != null)
                {
                    using (var readStream = new StreamReader(receiveStream, Encoding.Default))
                    {
                        string content = readStream.ReadToEnd();// HandlerHelper.ParseHtmlResponse(readStream.ReadToEnd(), "");
                        result.Add("Data", content);
                    }
                }
            }
            response.Close();

            return Content(result.ToString(), "application/json");
        }

        public ActionResult TestDatabase()
        {
            var result = new JObject();

            if (JP2ConfigHelper.DebugJp2HandlerRequests)
            {
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
            }

            return Content(result.ToString(), "application/json");
        }

        [HttpPost]
        public ActionResult TestUrl(string url)
        {
            HttpWebResponse response;
            var result = new JObject();

            if (JP2ConfigHelper.DebugJp2HandlerRequests && !string.IsNullOrEmpty(url))
            {
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
                catch (Exception e)
                {
                    result.Add("Error", e.Message);
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
                    }
                }
                response.Close();
            }

            return Content(result.ToString(), "application/json");
        }


        public ActionResult GetLog(int month, int year)
        {
            if (JP2ConfigHelper.DebugJp2HandlerRequests)
                return Json(LogHelper.GetLog(month, year), JsonRequestBehavior.AllowGet);
            else
                return Json("Set DebugJp2HandlerRequests to true in the web.config to view log, remember to set back to false when testing complete.", JsonRequestBehavior.AllowGet);
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