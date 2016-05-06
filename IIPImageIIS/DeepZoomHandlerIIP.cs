using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.SessionState;
using iNQUIRE.Helper;

namespace IIPImageIIS
{
    public class DeepZoomHandlerIIP : IHttpHandler, IRequiresSessionState
    {
        #region IHttpHandler implementations


        public bool IsReusable
        {
            get { return true; }
        }

        /// <summary>
        /// Method calls when client request the server
        /// </summary>
        /// <param name="context">HTTP context for client</param>
        public void ProcessRequest(HttpContext context)
        {
            var is_img_request = context.Request.Url.Query.Contains("_files/");
            string qs = context.Request.Url.Query;
            int tile_width = HandlerHelper.TileSize;

            string uri = string.Format("{0}{1}", HandlerHelper.ResolverUri, qs);

            if (!is_img_request)
            {
                uri = string.Format("{0}{1}", uri, ".dzi");
                var dzi_md_xml =  GetDziMetadata(uri, context.Request.QueryString["DeepZoom"], tile_width);
                context.Response.ContentType = "application/xml";
                context.Response.ContentEncoding = Encoding.UTF8;
                context.Response.Output.Write(dzi_md_xml);
            }
            else
            {
                // image requests can just be passed directly to the iip image server, and returned to the user (so we're still acting as a reverse proxy)
                var request = (HttpWebRequest)WebRequest.Create(uri);

                HttpWebResponse response;
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch (WebException)
                {
                    context.Response.StatusCode = 404;
                    context.Response.StatusDescription = "Not Found";
                    context.Response.Write("<h2>Page not found</h2>");
                    context.Response.End();
                    return;
                }

                using (Stream receiveStream = response.GetResponseStream())
                {
                    if (receiveStream != null)
                    {
                        context.Response.ContentType = "image/jpeg";
                        var buff = new byte[1024];
                        int bytes;
                        while ((bytes = receiveStream.Read(buff, 0, 1024)) > 0)
                            context.Response.OutputStream.Write(buff, 0, bytes);
                    }
                }
                response.Close();

                // image, so allow caching
                context.Response.CacheControl = "Public";
                context.Response.Expires = 10080;
            }

            context.Response.End();
        }
        #endregion


        private string GetDziMetadata(string dzi_metadata_uri, string id, int tile_width)
        {
            try
            {
                // effectively a pass through request, so all we're doing here is acting as a reverse proxy via this handler
                // (we'd have gone direct to the iip image server, not to this handler, if not in reverse proxy mode)
                var iiph = new iNQUIRE.Helper.IIPImageHelper();
                return iiph.ReadJpeg2000Metadata(dzi_metadata_uri, id); 
            }
            catch (Exception e)
            {
                return string.Format("<?xml version=\"1.0\" encoding=\"UTF-8\"?><Image TileSize=\"{0}\" Overlap=\"0\" Format=\"jpg\" xmlns=\"http://schemas.microsoft.com/deepzoom/2008\"><Size Width=\"1\" Height=\"1\"/></Image>", tile_width);
            }
        }
    }
}