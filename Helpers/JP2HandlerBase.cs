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

        public virtual string MakeUri(HttpContext context) 
        {
            appBaseUri = string.Format("{0}://{1}:{2}{3}", context.Request.Url.Scheme, context.Request.Url.Host, context.Request.Url.Port, context.Request.FilePath);

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

                var status_code = 500;
                var status_desc = "Internal server error";

                if (err_response != null)
                {
                    status_code = (int)err_response.StatusCode;
                    status_desc = err_response.StatusDescription;
                }

                LogHelper.StatsLog(null, "JP2HandlerBase.ProcessRequest()", String.Format("Failed, status code: {0} , status desc: {1}, Message: {2}, Uri: {3}", status_code, status_desc, ex.Message, remoteUrl), null, null);

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
