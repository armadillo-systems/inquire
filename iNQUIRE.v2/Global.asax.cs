using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.ComponentModel;
using System.Configuration;
using System.Threading;
using System.Text;
using System.Reflection;
//using Ninject;
//using Ninject.MVC;
//using Ninject.Web;
using iNQUIRE.Models;
using iNQUIRE.Helpers;
using SolrNet;
using System.Net;
using System.Web.Http;

namespace iNQUIRE
{
    public class MvcApplication : HttpApplication // Ninject.Web.Common.NinjectHttpApplication // NinjectHttpApplication
    {
        Queue<EmailExport> _emailQueue;
        int _emailQueueProcessDelayMS;
        string _exportEmailSubject;

        //protected override IKernel CreateKernel()
        //{
        //    System.Diagnostics.Debugger.Break();
        //    var kernel = new StandardKernel();
        //    kernel.Load(Assembly.GetExecutingAssembly());
        //    return kernel;
        //}

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            // adding this means http requests to this app won't work, so better make sure all http requests get rewritten to https (not currently working)
            // think about already sent out http links with url params eg shared items in deepzoom mode, should be ok if all http requests are redirected to https 
            //GlobalFilters.Filters.Add(new RequireHttpsAttribute()); 
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            //NinjectContainer.RegisterModules(NinjectModules.Modules);


            Console.WriteLine("Application_Start()");
            Helper.LogHelper.ErrorLogFileName = ConfigurationManager.AppSettings["ErrorLogFile"];
            Helper.LogHelper.LogFileDirectory = ConfigurationManager.AppSettings["LogFileDirectory"];

            ImageHelper.DjatokaHome = ConfigurationManager.AppSettings["DjatokaHome"];
            ImageHelper.JavaHome = ConfigurationManager.AppSettings["JavaHome"];
            ImageHelper.Jpeg2000Namespace = ConfigurationManager.AppSettings["Jpeg2000Namespace"];
            ImageHelper.Jpeg2000NamespaceReplace = ConfigurationManager.AppSettings["Jpeg2000NamespaceReplace"];
            ImageHelper.Jpeg2000Directory = ConfigurationManager.AppSettings["Jpeg2000Directory"];
            ImageHelper.ImageDirectory = ConfigurationManager.AppSettings["ImageDirectory"];
            ImageHelper.ImageFilenameAppend = ConfigurationManager.AppSettings["ImageFilenameAppend"];
            ImageHelper.ForceHttpForImageRequests = Convert.ToBoolean(ConfigurationManager.AppSettings["ForceHttpForImageRequests"]);

            Helper.JP2ConfigHelper.ApplicationBaseUri = ConfigurationManager.AppSettings["ApplicationBaseUri"];
            Helper.JP2ConfigHelper.ProxyResolverFile = ConfigurationManager.AppSettings["ProxyResolverFile"];
            Helper.JP2ConfigHelper.ResolverUri = ConfigurationManager.AppSettings["ResolverUri"];
            Helper.JP2ConfigHelper.ViewerUri = ConfigurationManager.AppSettings["ViewerUri"];
            Helper.JP2ConfigHelper.DeepZoomViewerFile = ConfigurationManager.AppSettings["DeepZoomViewerFile"];
            Helper.JP2ConfigHelper.DeepZoomQueryParameter = ConfigurationManager.AppSettings["DeepZoomQueryParameter"];
            Helper.JP2ConfigHelper.ZoomViewerHeightPx = Convert.ToInt32(ConfigurationManager.AppSettings["ZoomViewerHeightPx"]);
            Helper.JP2ConfigHelper.TileSize = Convert.ToInt32(ConfigurationManager.AppSettings["TileSize"]);
            Helper.JP2ConfigHelper.MediaDirectory = ConfigurationManager.AppSettings["MediaDirectoryRemote"];
            Helper.JP2ConfigHelper.DebugJp2HandlerRequests = Convert.ToBoolean(ConfigurationManager.AppSettings["DebugJp2HandlerRequests"]);

            iNQUIRE.ApplicationViewPage<object>.GoogleAnalyticsId = ConfigurationManager.AppSettings["GoogleAnalyticsId"];
            iNQUIRE.ApplicationViewPage<object>.FacebookAppId = ConfigurationManager.AppSettings["FacebookAppId"];
            iNQUIRE.ApplicationViewPage<object>.FacebookLike = Convert.ToBoolean(ConfigurationManager.AppSettings["FacebookLike"]);
            iNQUIRE.ApplicationViewPage<object>.FacebookUploadPhoto = Convert.ToBoolean(ConfigurationManager.AppSettings["FacebookUploadPhoto"]);
            iNQUIRE.ApplicationViewPage<object>.TwitterText = ConfigurationManager.AppSettings["TwitterText"];
            iNQUIRE.ApplicationViewPage<object>.TwitterHashtag = ConfigurationManager.AppSettings["TwitterHashtag"];
            iNQUIRE.ApplicationViewPage<object>.TwitterActivityCaption = ConfigurationManager.AppSettings["TwitterActivityCaption"];

            Helpers.SolrHelper.FacetsString = ConfigurationManager.AppSettings["Facets"];
            Helpers.SolrHelper.SortFieldsString = ConfigurationManager.AppSettings["SortFields"];

            // Controllers.DiscoverController.HyperlinkFields = new List<string>(ConfigurationManager.AppSettings["HyperlinkFields"].Split(new[] { "^" }, StringSplitOptions.RemoveEmptyEntries));
            Controllers.DiscoverController.ApplicationIdAspNet = new Guid(ConfigurationManager.AppSettings["ApplicationIdAspNet"]);
            Controllers.DiscoverController.ApplicationIdInquire = new Guid(ConfigurationManager.AppSettings["ApplicationIdInquire"]);
            Controllers.WebApi.WebApiControllerBase.ApplicationIdInquire = new Guid(ConfigurationManager.AppSettings["ApplicationIdInquire"]);
            Controllers.DiscoverController.ExportFilename = ConfigurationManager.AppSettings["ExportFilename"];
            Controllers.DiscoverController.SavedSearchesDisplayMax = Convert.ToInt32(ConfigurationManager.AppSettings["SavedSearchesDisplayMax"]);
            Controllers.DiscoverController.TouchDoubleClickDelayMs = Convert.ToInt32(ConfigurationManager.AppSettings["TouchDoubleClickDelayMs"]);
            Controllers.DiscoverController.OpenDeepZoomTouchIcon = ConfigurationManager.AppSettings["OpenDeepZoomTouchIcon"];
            Controllers.DiscoverController.AlwaysShowOpenDeepZoomTouchIcon = Convert.ToBoolean(ConfigurationManager.AppSettings["AlwaysShowOpenDeepZoomTouchIcon"]);
            Controllers.DiscoverController.FacebookShareHashtag = ConfigurationManager.AppSettings["FacebookShareHashtag"];
            Controllers.DiscoverController.UseTimeSince = Convert.ToBoolean(ConfigurationManager.AppSettings["UseTimeSince"]);

            Controllers.DiscoverController.SearchDebugParameters = ConfigurationManager.AppSettings["SearchDebugParameters"];
            Controllers.DiscoverController.SolrDebugParameters = ConfigurationManager.AppSettings["SolrDebugParameters"];
            Controllers.DiscoverController.IIPDebugParameters = ConfigurationManager.AppSettings["IIPDebugParameters"];
            Controllers.DiscoverController.DeepZoomDebugParameters = ConfigurationManager.AppSettings["DeepZoomDebugParameters"];

            Controllers.DiscoverController.Languages = MakeKeyValuePairStringStringListFromConfigString(ConfigurationManager.AppSettings["Languages"]);
            Controllers.DiscoverController.MultiLingualSolrFields = ConfigurationManager.AppSettings["MultiLingualSolrFields"].Split(new[] { "^" }, StringSplitOptions.RemoveEmptyEntries).ToList();

            ImageHelper.ExportImageWidth = Convert.ToInt32(ConfigurationManager.AppSettings["ExportImageWidth"]);
            ImageHelper.ExportImageHeight = Convert.ToInt32(ConfigurationManager.AppSettings["ExportImageHeight"]);
            ImageHelper.ExportMaxImages = Convert.ToInt32(ConfigurationManager.AppSettings["ExportMaxImages"]);

            Helper.EmailHelper.FromAddress = ConfigurationManager.AppSettings["FromEmailAddress"];
            _exportEmailSubject = ConfigurationManager.AppSettings["ExportEmailSubject"];
            Helper.EmailHelper.SmtpHost = ConfigurationManager.AppSettings["SMTPHost"];
            var port = ConfigurationManager.AppSettings["SMTPPort"];
            int port_num = 0;
            if (string.IsNullOrEmpty(port) == false)
                port_num = Convert.ToInt32(port);

            Helper.EmailHelper.SmtpPort = port_num;
            _emailQueueProcessDelayMS = Convert.ToInt32(ConfigurationManager.AppSettings["EmailQueueProcessDelayMS"]);

            SolrRepository.HyperlinkTargetBlank = Convert.ToBoolean(ConfigurationManager.AppSettings["HyperlinkTargetBlank"]);

            // parse fields to mark up hyperlinks
            SolrRepository.HyperlinkFields = MakeKeyValuePairStringBoolListFromConfigString(ConfigurationManager.AppSettings["HyperlinkFields"]);

            SolrRepository.NakedHyperlinkPrefix = ConfigurationManager.AppSettings["NakedHyperlinkPrefix"];
            SolrRepository.FacetLimit = Convert.ToInt32(ConfigurationManager.AppSettings["FacetLimit"]);
            SolrRepository.MultiFacetConstraints = Convert.ToBoolean(ConfigurationManager.AppSettings["MultiFacetConstraints"]);
            SolrRepository.XmlPath = Server.MapPath(@"~\App_Data\");
            InqItemBase.ViewItemBaseUri = ConfigurationManager.AppSettings["ViewItemBaseUri"];

            // AreaRegistration.RegisterAllAreas();

            // RegisterGlobalFilters(GlobalFilters.Filters);
            // RegisterRoutes(RouteTable.Routes);

            // IMPORTANT: Supply your Solr specific data class here, eg:
            // 
            // SolrNet.Startup.Init<YourItemDataClass>(ConfigurationManager.AppSettings["SolrUri"]);

            // You data class should be derived from InqItemBase or InqItemImageMetadataWidthAndHeightBase
            // see InqItemBod.cs, InqItemArmNode.cs as examples


            // this is annoying, Solr .net throws an error if you try to supply it with an interface or abstract class, so can't use eg ninject DI?
            // Startup.Init<InqItemXml>(ConfigurationManager.AppSettings["SolrUriXml"]);
            //SolrNet.Startup.Init<InqItemArmNode>(ConfigurationManager.AppSettings["SolrUri"]);
            //SolrNet.Startup.Init<InqItemRKD>(ConfigurationManager.AppSettings["SolrUri"]);
            //var inq_type = WebApiConfig.UnityContainer.Resolve(typeof(InqItemBase), null, null);
            SolrNet.Startup.Init<InqItemBodIIIF>(ConfigurationManager.AppSettings["SolrUri"]);
            // Startup.Init<InqItemBod>(ConfigurationManager.AppSettings["SolrUri"]);

            // throw new Exception("moo!");
            // SolrHelper.Setup();

            // System.Diagnostics.Debugger.Break();
            _emailQueue = new Queue<EmailExport>();
            var email_worker = new BackgroundWorker(); // email generation background thread, shared by all users, application scope
            email_worker.DoWork += email_DoWork;
            email_worker.RunWorkerAsync();

            Application["email_queue"] = _emailQueue;
        }

        private static List<KeyValuePair<string, bool>> MakeKeyValuePairStringBoolListFromConfigString(string config_str)
        {
            var kvp_list = new List<KeyValuePair<string, bool>>();
            var f = config_str.Split(new[] { "^" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string s in f)
            {
                var f2 = s.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                var kvp = new KeyValuePair<string, bool>(f2[0], Convert.ToBoolean(f2[1]));
                kvp_list.Add(kvp);
            }
            return kvp_list;
        }

        private static List<KeyValuePair<string, string>> MakeKeyValuePairStringStringListFromConfigString(string config_str)
        {
            var kvp_list = new List<KeyValuePair<string, string>>();
            var f = config_str.Split(new[] { "^" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string s in f)
            {
                var f2 = s.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                var kvp = new KeyValuePair<string, string>(f2[0], f2[1]);
                kvp_list.Add(kvp);
            }
            return kvp_list;
        }

        async void email_DoWork(object sender, DoWorkEventArgs e)
        {
            // System.Diagnostics.Debugger.Break();
            while (true)
            {
                Thread.Sleep(_emailQueueProcessDelayMS);

                if (_emailQueue.Count > 0)
                {
                    var email_export = _emailQueue.Dequeue();

                    var email_resources = new List<System.Net.Mail.LinkedResource>();

                    var email_html = new StringBuilder("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">");
                    email_html.Append("<html xmlns=\"http://www.w3.org/1999/xhtml\"><head><meta http-equiv=\"Content-Type\" content=\"text/html;charset=utf-8\" /><title>iNQUIRE Items Export</title>");
                    email_html.Append("<style type=\"text/css\">span.label { font-weight: bold; } div.desc { margin-bottom: 10px; }</style></head>");
                    email_html.Append("<body><h1><u>iNQUIRE Items Export</u></h1>");

                    email_html.Append(String.Format("<div>{0}</div>", email_export.Message));

                    int count = 0;

                    foreach (ExportItem ei in email_export.Items)
                    {
                        email_html.Append("<div><table width=\"950\"><tr>");

                        System.Net.Mail.LinkedResource lr;
                        string content_id = null;

                        if (!string.IsNullOrEmpty(ei.ImageUri))
                        {
                            lr = new System.Net.Mail.LinkedResource(ImageHelper.GetImageStream(ei.ImageUri, email_export.ImageNotFoundUri));
                            content_id = String.Format("image{0}", Guid.NewGuid().ToString().Replace("-", ""));
                            lr.ContentId = content_id;
                            lr.ContentType.MediaType = "image/jpeg";
                            email_resources.Add(lr);
                        }

                        email_html.Append(ei.Item.ExportHtml(content_id, ei.LanguageId));
                        email_html.Append("</td></tr></table></div>");

                        if (count < email_export.Items.Count - 1)
                            email_html.Append("<hr />");
                        count++;
                    }

                    email_html.Append("</body></html>");

                    await Helper.EmailHelper.SendEmail(email_export.EmailTo, _exportEmailSubject, email_html.ToString(), email_resources);
                }
            }
        }
    }
}
