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
            appBaseUri = string.Format("http://{0}{1}", context.Request.Url.Host, context.Request.FilePath);

            string new_uri = context.Request.FilePath.Contains("viewer") ? HandlerHelper.ViewerUri : HandlerHelper.ResolverUri;
            return context.Request.Url.AbsoluteUri.Replace(appBaseUri, new_uri);
        }

        /// <summary>
        /// Method calls when client request the server
        /// </summary>
        /// <param name="context">HTTP context for client</param>
        public void ProcessRequest(HttpContext context)
        {
            var remoteUrl = MakeUri(context);

            var is_json = IsJson(remoteUrl);
            var is_jpeg = IsJpeg(remoteUrl);

            //create the web request to get the remote stream
            var request = (HttpWebRequest)WebRequest.Create(remoteUrl);

            //request.Credentials = CredentialCache.DefaultCredentials;

            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                //remote url not found, send 404 to client 
                context.Response.StatusCode = (int)((HttpWebResponse)ex.Response).StatusCode;
                context.Response.StatusDescription = "Not Found";
                context.Response.Write("<h2>Page not found</h2>");
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

                        if (is_jpeg)
                            context.Response.ContentType = "image/jpeg";

                        if (is_json)
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
