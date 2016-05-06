using System;
using System.Configuration;
using System.Net;
using System.Text;

namespace iNQUIRE.Helper
{
    public class DjatokaHelper : JP2HelperBase
    {
        public override string GetJpeg2000Metadata(string id, bool use_reverse_proxy)
        {
            var djatoka_resolver_file = use_reverse_proxy ? String.Format("{0}{1}", ApplicationBaseUri, ProxyResolverFile) : ResolverUri;

            var uri = String.Format("{0}?url_ver=Z39.88-2004&rft_id={1}&svc_id=info:lanl-repo/svc/getMetadata", djatoka_resolver_file, id); // HttpUtility.UrlEncode(id)
            // LogHelper.StatsLog(null, "GetJpeg2000Metadata()", uri, null, null);
            return ReadJpeg2000Metadata(uri, id);
        }

        public override string MakeJP2ImageUri(string jp2resolver, string id, int level, int w, int h)
        {
            return String.Format("{0}?url_ver=Z39.88-2004&rft_id={1}&svc_id=info:lanl-repo/svc/getRegion&svc_val_fmt=info:ofi/fmt:kev:mtx:jpeg2000&svc.format=image/jpeg&svc.level={2}&svc.rotate=0&svc.region=0,0,{3},{4}", jp2resolver, id, level, h, w);
        }
    }
}
