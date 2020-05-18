using System;
using System.Configuration;

namespace iNQUIRE.Helper
{
    public static class HandlerHelper
    {
        // Warning: The config these values come from is the web.config of the calling app, so values
        // for these appSettings need to be eg. in the inquire web.config too, else values will be null when
        // calling from that application (as values won't be found!), which is a bit confusing really
        public static string ResolverUri = ConfigurationManager.AppSettings["ResolverUri"];
        public static string ViewerUri = ConfigurationManager.AppSettings["ViewerUri"];
        public static int TileSize = Convert.ToInt32(ConfigurationManager.AppSettings["TileSize"]);

        public static bool DebugJp2HandlerRequests = Convert.ToBoolean(ConfigurationManager.AppSettings["DebugJp2HandlerRequests"]);
        public static string SearchDebugParameters = ConfigurationManager.AppSettings["SearchDebugParameters"];
        public static string SolrDebugParameters = ConfigurationManager.AppSettings["SolrDebugParameters"];
        public static string IIPDebugParameters = ConfigurationManager.AppSettings["IIPDebugParameters"];
        public static string DeepZoomDebugParameters = ConfigurationManager.AppSettings["DeepZoomDebugParameters"];

        /// <summary>
        /// Get the remote URL to call
        /// </summary>
        /// <param name="url">URL get by client</param>
        /// <returns>Remote URL to return to the client</returns>
        public static string ParseURL(string url)
        {
            if (url.IndexOf("http/") >= 0)
            {
                string externalUrl = url.Substring(url.IndexOf("http/"));
                return externalUrl.Replace("http/", "http://");
            }

            return url;
        }

        /// <summary>
        /// Parse HTML response for update links and images sources
        /// </summary>
        /// <param name="html">HTML response</param>
        /// <param name="appPath">Path of application for replacement</param>
        /// <returns>HTML updated</returns>
        public static string ParseHtmlResponse(string html, string appPath)
        {
            html = html.Replace("\"/", "\"" + appPath + "/");
            html = html.Replace("'/", "'" + appPath + "/");
            html = html.Replace("=/", "=" + appPath + "/");

            return html;
        }
    }
}