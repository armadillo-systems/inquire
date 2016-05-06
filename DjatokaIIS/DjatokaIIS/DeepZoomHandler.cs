using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.SessionState;
using iNQUIRE.Helper;

namespace DjatokaIIS
{
    public class DeepZoomHandler : IHttpHandler, IRequiresSessionState
    {
        #region IHttpHandler implementations

        // private readonly iNQUIRE.Helper.IJP2Helper _IJP2Helper;

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
            string qs = context.Request.QueryString.ToString();
            int tile_width = HandlerHelper.TileSize;

            if (!is_img_request)
            {
                var dzi_md = GetDziMetadata(context.Request.QueryString["rft_id"], tile_width);
                context.Session["ImageWidth"] = dzi_md.Width;
                context.Session["ImageHeight"] = dzi_md.Height;
                context.Session["Levels"] = dzi_md.Levels;
                var xml = dzi_md.Xml;

                context.Response.ContentType = "application/xml";
                context.Response.ContentEncoding = Encoding.UTF8;

                context.Response.Output.Write(xml);
            }
            else
            {
                string url = processImageRequest(qs,
                                                tile_width,
                                                Convert.ToInt32(context.Session["ImageWidth"]),
                                                Convert.ToInt32(context.Session["ImageHeight"]),
                                                Convert.ToInt32(context.Session["Levels"]));

                var request = (HttpWebRequest)WebRequest.Create(url);

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

        private string processImageRequest(string query, int tile_width, int width_max, int height_max, int levels)
        {
            string id;

            query = Uri.UnescapeDataString(query);
            // work out which dzi level and tile has been requested
            const string dzi_ident_frag = "_files/";
            var pos = query.IndexOf(dzi_ident_frag);
            var dzi_info = query.Remove(0, pos + dzi_ident_frag.Length);
            string[] dzi_frags = dzi_info.Split('/');
            string[] tile_frags = dzi_frags[1].Remove(dzi_frags[1].Length - 4).Split('_');

            int resolution = Convert.ToInt32(dzi_frags[0]);
            int x = Convert.ToInt32(tile_frags[0]);
            int y = Convert.ToInt32(tile_frags[1]);

            int numResolutions = levels + 1; // eg 6 djatoka levels go from 0 to 6, so actually 7
            var dzi_res = (int)Math.Ceiling(Math.Log(Math.Max(width_max, height_max), 2));

            // extract djatoka id
            id = query.Remove(pos); // strip from "_files/" to end

            // Take into account the extra zoom levels required by the DeepZoom spec
            resolution = resolution - (dzi_res - numResolutions) - 1;

            if (resolution < 0)
                resolution = 0;

            if (resolution > numResolutions)
                resolution = numResolutions - 1;

            // Get the width and height for the requested resolution
            double d = Math.Pow(2, numResolutions - resolution - 1);
            int width = Convert.ToInt32(Math.Ceiling(width_max / d));
            int height = Convert.ToInt32(Math.Ceiling(height_max / d));

            // Get the width of the tiles and calculate the number of tiles in each direction
            int rem_x = width % tile_width;
            int rem_y = height % tile_width;
            int tiles_x = (width / tile_width) + (rem_x == 0 ? 0 : 1);
            int tiles_y = (height / tile_width) + (rem_y == 0 ? 0 : 1);

            double x_float = Convert.ToDouble(x) / tiles_x;
            double y_float = Convert.ToDouble(y) / tiles_y;

            //int x_offset = (width_max / tiles_x) * x;
            //int y_offset = (height_max / tiles_y) * y;

            #region calculations based on djatoka pixels
            //if((tiles_x > 1) && (rem_x > 0) && ((tiles_x - 1) == x))
            //    x_offset += (((tiles_x * tw) - width) / tiles_x) * (width_max / width);

            //if ((tiles_y > 1) && (rem_y > 0) && ((tiles_y - 1) == y))
            //    y_offset += (((tiles_y * tw) - width) / tiles_y) * (height_max / height);

            // Calculate the tile index for this resolution from our x, y
            // int tile = y*ntlx + x;
            #endregion

            // if it's a partial tile we have a problem, as if you give djatoka the tile width and what you would assume is
            // the correct x offset it will actually return an image deeper to the left than you actually want,
            // so to get round this you need to offset much more to the right, in fact by the amounts below...
            #region calculations based on djatoka float values
            if ((tiles_x > 1) && (rem_x > 0) && (x > 0))
                x_float = (tile_width * x) / (Double)width;

            if ((tiles_y > 1) && (rem_y > 0) && (y > 0))
                y_float = (tile_width * y) / (Double)height;

            #endregion

            int level = resolution;

            var region = string.Format("{0},{1},{2},{2}", y_float, x_float, tile_width); // y_float, x_float, tw);

            return string.Format("{0}?url_ver=Z39.88-2004&{1}&svc_id=info:lanl-repo/svc/getRegion&svc_val_fmt=info:ofi/fmt:kev:mtx:jpeg2000&svc.format=image/jpeg&svc.level={2}&svc.rotate=0&svc.region={3}", HandlerHelper.ResolverUri, id, level, region);
        }

        private DziMetaData GetDziMetadata(string id, int tile_width)
        {
            // get info from djatoka server, make into dzi xml
            // could possibly get ninject working here using some kind of IHttpHandler factory
            var djah = new iNQUIRE.Helper.DjatokaHelper();
            var json = djah.GetJpeg2000Metadata(id, false);

            //var json = _IJP2Helper.GetJpeg2000Metadata(id, false);
            
            json = json.Replace(@"\", @"\\");
            var ser = new JavaScriptSerializer();
            var md = ser.Deserialize<ImageMetadata>(json);
            return new DziMetaData(string.Format("<?xml version=\"1.0\" encoding=\"UTF-8\"?><Image TileSize=\"{2}\" Overlap=\"0\" Format=\"jpg\" xmlns=\"http://schemas.microsoft.com/deepzoom/2008\"><Size Width=\"{0}\" Height=\"{1}\"/></Image>", md.Width, md.Height, tile_width), md.Width, md.Height, md.Levels);
        }
    }

    public struct DziMetaData
    {
        public int Width, Height, Levels;
        public string Xml;

        public DziMetaData(string xml, int w, int h, int levels)
        {
            Xml = xml;
            Width = w;
            Height = h;
            Levels = levels;
        }
    }

/*    public class JP2HelperModule : NinjectModule
    {
        public override void Load()
        {
            Bind<iNQUIRE.Helper.IJP2Helper>().To<iNQUIRE.Helper.DjatokaHelper>();
        }
    }*/
}