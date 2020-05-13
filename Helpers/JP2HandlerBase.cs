using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;
using System.IO;

namespace iNQUIRE.Helper
{
    public abstract class JP2HandlerBase : IHttpHandler
    {

        #region IHttpHandler implementations

        public bool IsReusable
        {
            get { return true; }
        }

        public abstract bool IsJson(string url);
        public abstract bool IsJpeg(string url);

        public abstract string ProxyFixHTML(string content);

        protected string appBaseUri { get; set; }

        private string MakeAppBaseUri(HttpContext context)
        {
            return string.Format("{0}://{1}:{2}{3}", context.Request.Url.Scheme, context.Request.Url.Host, context.Request.Url.Port, context.Request.FilePath);
        }

        public virtual string MakeUri(HttpContext context) 
        {
            appBaseUri = MakeAppBaseUri(context);
            string new_uri = context.Request.FilePath.Contains("viewer") ? HandlerHelper.ViewerUri : HandlerHelper.ResolverUri;
            return context.Request.Url.AbsoluteUri.Replace(appBaseUri, new_uri);
        }

        /// <summary>
        /// Method calls when client request the server
        /// </summary>
        /// <param name="context">HTTP context for client</param>
        public void ProcessRequest(HttpContext context)
        {
            string remoteUrl = null;
            HttpWebResponse response;

            try
            {
                remoteUrl = MakeUri(context);

                // we've received a Url request and our handler has intercepted the request
                // and attempted to substitute the viewer/resolver Uri, but we've ended up
                // with the same Url, if we request it again we will just get stuck in a request loop
                if (remoteUrl.ToLower().CompareTo(context.Request.Url.AbsoluteUri.ToLower()) == 0)
                {
                    var msg = "JP2HandlerBase.ProcessRequest() source and destination Urls are the same";
                    //var r = new HttpWebResponse(null, null, null)
                    //{
                    //    r.StatusCode = 500,
                    //    r.StatusDescription = "Internal server error"
                    //};
                    throw new WebException(msg);
                }

                //create the web request to get the remote stream
                var request = (HttpWebRequest)WebRequest.Create(remoteUrl);

                //request.Credentials = CredentialCache.DefaultCredentials;

                // throw new WebException("moo!");
                response = (HttpWebResponse)request.GetResponse();

                if (HandlerHelper.DebugJp2HandlerRequests)
                    LogHelper.StatsLog(null, "JP2HandlerBase.ProcessRequest()", String.Format("Ok, status code: {0} , status desc: {1}, Uri: {2}", response.StatusCode, response.StatusDescription, remoteUrl), null, null);
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

                LogHelper.StatsLog(null, "JP2HandlerBase.ProcessRequest()", String.Format("Failed, Response status code: {0} , Response status desc: {1}, WebExceptionMessage: {2}, HandlerHelper.ViewerUri: {3}, HandlerHelper.ResolverUri: {4}, context.Request.Url.AbsoluteUri: {5}, Constructed remoteUrl: {6}", status_code, status_desc, ex.Message, HandlerHelper.ViewerUri, HandlerHelper.ResolverUri, context.Request.Url.AbsoluteUri, remoteUrl), null, null);

                context.Response.StatusCode = status_code;
                context.Response.StatusDescription = status_desc ;
                context.Response.Write("<h2>Error getting JP2 metadata</h2>");
                context.Response.End();
                return;
            }

            using (Stream receiveStream = response.GetResponseStream())
            {
                if (receiveStream != null)
                {
                    if ((response.ContentType.ToLower().IndexOf("html") >= 0) ||
                        (response.ContentType.ToLower().IndexOf("javascript") >= 0))
                    {
                        //this response is HTML Content, so we must parse it
                        using (var readStream = new StreamReader(receiveStream, Encoding.Default))
                        {
                            string content = HandlerHelper.ParseHtmlResponse(readStream.ReadToEnd(),
                                                                             context.Request.ApplicationPath);
                            //write the updated HTML to the client
                            int p = context.Request.FilePath.LastIndexOf("/");
                            var app_base = context.Request.FilePath.Remove(p);
                            context.Response.Write(ProxyFixHTML(content));
                        }
                    }
                    else
                    {
                        //the response is not HTML Content

                        if (IsJpeg(remoteUrl))
                            context.Response.ContentType = "image/jpeg";

                        if (IsJson(remoteUrl))
                        {
                            context.Response.ContentType = "application/json";
                            context.Response.ContentEncoding = Encoding.UTF8;
                        }

                        var buff = new byte[1024];
                        int bytes;
                        while ((bytes = receiveStream.Read(buff, 0, 1024)) > 0)
                        {
                            //Write the stream directly to the client 
                            context.Response.OutputStream.Write(buff, 0, bytes);
                        }
                    }
                }
            }

            response.Close();
            context.Response.End();
        }
        #endregion
    }
}
