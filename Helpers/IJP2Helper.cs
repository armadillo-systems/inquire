using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iNQUIRE.Helper
{
    public interface IJP2Helper
    {
        string ApplicationBaseUri { get; } // base Uri of application eg http://localhost/inquire/
        string ResolverUri { get; } // real djatoka uri, can be used to bypass the reverse proxy, if desired

        string ViewerUri { get; }
        string DeepZoomViewerFile { get; } // same as for ProxyResolverFile above, but for dzi
        int ZoomViewerHeightPx { get; }
        string MediaDirectory { get; }

        string ProxyResolverFile { get; } // filename to be used to construct the request which will be intercepted and reverse proxied by the IIS handler, eg http://localhost/inquire/djatoka.dja
        // string ProxyViewerFile { get; }


        // string ViewerReverseProxy { get; }
        string ResolverReverseProxy { get; }
        string DeepZoomViewerReverseProxy { get; }
        string DeepZoomQueryParameter { get; }

        string GetJpeg2000Metadata(string id, bool use_reverse_proxy);
        string GetImageUri(ImageMetadata imd, string media_directory, string djatoka_resolver, double w_img, double h_img);
    }
}
