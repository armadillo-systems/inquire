using iNQUIRE.Helper;
using System;
using System.Web;
using System.Configuration;

namespace IIIFIIS
{
    public class IIIFHandler : JP2HandlerBase
    {
        // IIP Image server (Windows?) bug: requesting default.jpg returns an error: quality can only be numeric or certain values,
        // doesn't accept "default" even though it should do under the iiif spec so "default" will be replaced with this value instead
        // taken from web.config
        public string IIIFDefaultQuality = ConfigurationManager.AppSettings["IIIFDefaultQuality"];

        public override bool IsJpeg(string url)
        {
            return !string.IsNullOrEmpty(url) ? url.EndsWith(".jpg", StringComparison.CurrentCultureIgnoreCase) : false;
        }

        public override bool IsJson(string url)
        {
            return !string.IsNullOrEmpty(url) ? url.EndsWith(".json", StringComparison.CurrentCultureIgnoreCase) : false;
        }

        public override string ProxyFixHTML(string content)
        {
            throw new System.NotImplementedException();
        }

        public override string MakeUri(HttpContext context)
        {
            //http://your.server/iiif/image.tif/full/400,400,90,default.jpg
            //http://www.example.org/image-service/abcd1234/full/full/0/default.jpg
            //http://62.49.109.149/imageserver-test/iipsrv.fcgi?IIIF=/demo.jp2/100,400,100,100/46,46/0/50.jpg
            //http://62.49.109.149/imageserver-test/iipsrv.fcgi?IIIF=/demo.jp2/full/,625/0/50.jpg
            // RewriteRule ^/iiif/$ /fcgi-bin/iipsrv.fcgi?IIIF=
            // http://62.49.109.149/imageserver-test/iipsrv.fcgi?IIIF=/demo.jp2/full/,625/0/50.jpg
            // http://ARMSERV/imageserver-test/iipsrv.fcgi

            if (context.Request.Url.AbsolutePath.Contains("/iiif/") == false)
                return null;

            string[] rsplit = context.Request.Url.AbsoluteUri.Split(new[] { "/iiif/" }, StringSplitOptions.None);

            if (rsplit.Length != 2)
                return null;

            if (rsplit[1].EndsWith("default.jpg", StringComparison.CurrentCultureIgnoreCase))
                rsplit[1] = rsplit[1].Replace("default.jpg", String.Format("{0}.jpg", IIIFDefaultQuality));

            return string.Format("{0}?IIIF={1}", JP2ConfigHelper.ResolverUri, rsplit[1]);
        }

    }
}