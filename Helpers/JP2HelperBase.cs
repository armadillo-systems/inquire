using System;
using System.Configuration;
using System.Net;
using System.Text;
using System.Collections.Generic;

namespace iNQUIRE.Helper
{
    public abstract class JP2HelperBase : IJP2Helper
    {
        public string ApplicationBaseUri { get { return JP2ConfigHelper.ApplicationBaseUri; } }
        public string ResolverUri { get { return JP2ConfigHelper.ResolverUri; } }

        public string ViewerUri { get { return JP2ConfigHelper.ViewerUri; } }
        public string DeepZoomViewerFile { get { return JP2ConfigHelper.DeepZoomViewerFile; } }
        public string DeepZoomQueryParameter { get { return JP2ConfigHelper.DeepZoomQueryParameter; } }
        public int ZoomViewerHeightPx { get { return JP2ConfigHelper.ZoomViewerHeightPx; } }
        public string MediaDirectory { get { return JP2ConfigHelper.MediaDirectory; } }

        public string ProxyResolverFile { get { return JP2ConfigHelper.ProxyResolverFile; } }
        // public string ProxyViewerFile { get { return JP2ConfigHelper.ProxyViewerFile; } }

        public abstract string MakeJP2ImageUri(string jp2resolver, string id, int level, int w, int h);

        public abstract string GetJpeg2000Metadata(string id, bool use_reverse_proxy);

        /*private string _viewerReverseProxy;
        public string ViewerReverseProxy
        {
            get
            {
                if (String.IsNullOrEmpty(_viewerReverseProxy))
                    _viewerReverseProxy = String.Format("{0}{1}", ApplicationBaseUri, ProxyViewerFile);
                return _viewerReverseProxy;
            }
        }*/

        private string _resolverReverseProxy;
        public string ResolverReverseProxy
        {
            get
            {
                if (String.IsNullOrEmpty(_resolverReverseProxy))
                    _resolverReverseProxy = String.Format("{0}{1}", ApplicationBaseUri, ProxyResolverFile);
                return _resolverReverseProxy;
            }
        }

        private string _deepZoomViewerReverseProxy;
        public string DeepZoomViewerReverseProxy
        {
            get
            {
                if (String.IsNullOrEmpty(_deepZoomViewerReverseProxy))
                    _deepZoomViewerReverseProxy = String.Format("{0}{1}", ApplicationBaseUri, DeepZoomViewerFile);
                return _deepZoomViewerReverseProxy;
            }
        }

        public virtual string ReadJpeg2000Metadata(string metadata_uri, string id)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(metadata_uri);

                try
                {
                    var response = (HttpWebResponse)request.GetResponse(); // async here? as this will be blocking, and if djatoka server slow or down then thread poll will lock up
                    // but if async will return straight away so hmm we do need this data immediately for presenting the thumbnail..

                    #region async code
                    //HttpWebRequest webRequest;

                    //void StartWebRequest()
                    //{
                    //    webRequest.BeginGetResponse(new AsyncCallback(FinishWebRequest), null);
                    //}

                    //void FinishWebRequest(IAsyncResult result)
                    //{
                    //    webRequest.EndGetResponse(result);
                    //}
                    #endregion

                    using (var resStream = response.GetResponseStream())
                    {
                        // throw new Exception("test");
                        int count = 0;
                        var sb = new StringBuilder();
                        var buf = new byte[8192];

                        do
                        {
                            // fill the buffer with data
                            if (resStream != null)
                                count = resStream.Read(buf, 0, buf.Length);

                            // make sure we read some data
                            if (count != 0) // translate from bytes to ASCII text continue building the string
                                sb.Append(Encoding.UTF8.GetString(buf, 0, count));

                        } while (count > 0); // any more data to read?

                        return sb.ToString();
                    }

                }
                catch (Exception ex)
                {
                    LogHelper.StatsLog(null, "ReadJpeg2000Metadata()", String.Format("Failed for id {0}: , Message: {1}, Uri: {2}", id, ex.Message, metadata_uri), null, null);
                    throw new Exception("Couldn't get JPEG2000 Metadata", ex);
                }
            }
            catch (Exception e)
            {
                return String.Format("{{ 'identifier': '{0}', 'imagefile': '{0}', 'width': '1', 'height': '1', 'dwtLevels': '0', 'levels': '0', 'compositingLayerCount': '0' }}", id);
            }
        }

        public virtual String GetImageUri(ImageMetadata imd, string media_directory, string jp2resolver, double w_img, double h_img)
        {
            string img_src = "";

            if (imd == null)
                return img_src;

            if (imd.Width > 1 )
            {
                double w = imd.Width;
                double h = imd.Height;
                int levels = imd.Levels;

                double ar = w / h;


                if (w > h)
                    h_img = Math.Round((double)w_img / ar); // landscape
                else
                    w_img = Math.Round((double)h_img * ar); // portrait or square

                // calculate level needed, as only using probably around 300-500px images can just take the level
                // above the level which is below the desired size, wouldn't be good logic for requesting higher
                // res images, as the jump would be too great (eg small jump from 192 to 394px but much bigger eg 1536 to 3072px)
                var level = 0;

                for (var i = levels; i > -1; i--)
                {
                    // alert('i: ' + i + ', w_img: ' + w_img + ', w: ' + w + ', h_img: ' + h_img + ', h: ' + h);
                    if ((w < w_img) && (h < h_img))
                    {
                        level = i + 1;
                        break;
                    }
                    w = w / 2;
                    h = h / 2;
                }

                int r_x = Convert.ToInt32(w * 2);
                int r_y = Convert.ToInt32(h * 2);

                img_src = MakeJP2ImageUri(jp2resolver, imd.Identifier, level, r_y, r_x);
            }
            else
            {
                var file = imd.Imagefile;
                if (IsAudioOrVideo(file))
                    file = System.IO.Path.ChangeExtension(file, ".jpg"); // replace audio/video file extension with .jpg so thumbnail can be read for metadata

                img_src = String.Format("{0}{1}.ashx?maxwidth={2}&amp;maxheight={3}", media_directory, file, w_img, h_img);
            }

            return img_src;
        }

        public static bool IsAudioOrVideo(string filename)
        {
            var vid_aud_ext = new List<string>() { ".ogg", ".ogv", ".oga", ".wmv", ".mp4", ".mp3", ".mov", ".webm" };
            foreach (string ext in vid_aud_ext)
            {
                if (filename.EndsWith(ext))
                    return true;
            }
            return false;
        }
    }
}
