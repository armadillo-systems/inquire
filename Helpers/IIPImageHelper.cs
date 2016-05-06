using System;
using System.Configuration;
using System.Net;
using System.Text;

namespace iNQUIRE.Helper
{
    public class IIPImageHelper : JP2HelperBase 
    {

        public override string GetJpeg2000Metadata(string id, bool use_reverse_proxy)
        {
            var iip_resolver_file = use_reverse_proxy ? String.Format("{0}{1}", ApplicationBaseUri, ProxyResolverFile) : ResolverUri;
            var uri = String.Format("{0}?FIF={1}&obj=IIP,1.0&obj=Max-size&obj=Tile-size&obj=Resolution-number", iip_resolver_file, id); // HttpUtility.UrlEncode(id)
            // LogHelper.StatsLog(null, "GetJpeg2000Metadata()", uri, null, null);
            var md = ReadJpeg2000Metadata(uri, id);
            
            if (md.Contains("{ 'identifier':"))
                return md;

            return parseIIPImageData(id, md);
        }

        public override string MakeJP2ImageUri(string jp2resolver, string id, int level, int w, int h)
        {
            string s;
            if (w > h)
                s = String.Format("WID={0}", w);
            else
                s = String.Format("HEI={0}", h);

            return String.Format("{0}?FIF={1}&{2}&RGN=0,0,1,1&CVT=jpeg'", jp2resolver, id, s);
        }

        private string parseIIPImageData(string id, string iip_md)
        {
            // parse IIP image metadata to json djatoka format,
            // as it's a more comprehensive (in terms of fields)
            // and interoperable format which we were using anyway (as originally djatoka was the only image server supported)
            string[] md_arr = iip_md.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            // image size
            string[] img_size = md_arr[1].Split(new[] { ":" }, StringSplitOptions.None);
            string[] img_sizes = img_size[1].Split(new[] { " " }, StringSplitOptions.None);

            // levels
            string[] levels_arr = md_arr[3].Split(new[] { ":" }, StringSplitOptions.None);
            var levels = Convert.ToUInt32(levels_arr[1]);
            levels++; // iip levels seem to be one less than djatoka reports

            var md = String.Format("{{ 'identifier': '{0}', 'imagefile': '{0}', 'width': '{1}', 'height': '{2}', 'dwtLevels': '{3}', 'levels': '{3}', 'compositingLayerCount': '0' }}", id, img_sizes[0], img_sizes[1], levels);
            return md;
        }
    }
}
