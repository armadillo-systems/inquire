using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace iNQUIRE
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{resource}.dja/{*pathInfo}");
            routes.IgnoreRoute("{resource}.dzi/{*pathInfo}");
            routes.IgnoreRoute("{resource}.iip/{*pathInfo}");
            routes.IgnoreRoute("iiif/{*pathInfo}");

            routes.MapRoute(
                "ViewItem",
                "v/{id}",
                new { controller = "Discover", action = "ViewItem", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
        }
    }
}
