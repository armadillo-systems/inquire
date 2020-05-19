﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iNQUIRE.Helper
{
    /// <summary>
    /// Static class to hold configuration values, as implementations of IJP2Handler can't have static variables (?).
    /// </summary>
    public static class JP2ConfigHelper
    {
        public static string ApplicationBaseUri { get; set; }
        public static string ResolverUri { get; set; }
        public static string ViewerUri { get; set; }
        public static string DeepZoomViewerFile { get; set; }
        public static string DeepZoomQueryParameter { get; set; }
        public static int ZoomViewerHeightPx { get; set; }
        public static int TileSize { get; set; }
        public static string MediaDirectory { get; set; }
        public static string ProxyResolverFile { get; set; }
        public static bool DebugJp2HandlerRequests { get; set; }
    }
}
