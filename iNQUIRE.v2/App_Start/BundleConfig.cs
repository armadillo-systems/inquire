using System.Web;
using System.Web.Optimization;

namespace iNQUIRE
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jquery-ui").Include(
                        "~/Scripts/jquery-ui*"));

            bundles.Add(new ScriptBundle("~/bundles/jquery.menu-aim").Include(
                        "~/Scripts/jquery.menu-aim.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/openid").Include(
                        "~/Scripts/openid-*"));

            bundles.Add(new ScriptBundle("~/bundles/knockout").Include(
                        "~/Scripts/knockout-{version}.js",
                        "~/Scripts/knockout.mapping-{version}.js",
                        "~/Scripts/knockout.validation.js"));

            bundles.Add(new ScriptBundle("~/bundles/openid").Include("~/Scripts/openid*"));

            // bundles.Add(new ScriptBundle("~/bundles/materialize").Include("~/Scripts/materialize/materialize.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/sammy").Include("~/Scripts/sammy-*"));

            bundles.Add(new ScriptBundle("~/bundles/masonry").Include("~/Scripts/masonry.pkgd.min.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/reset.css",
                      "~/Content/bootstrap.css",
                      "~/Content/bootstrap-social.css",
                      "~/Content/font-awesome.css",
                      "~/Content/jquery-ui.min.css",
                      "~/Content/jquery-ui.structure.min.css",
                      "~/Content/jquery-ui.theme.min.css",
                      "~/Content/openid.css",
                      "~/Content/openid-shadow.css",
                      //"~/Content/overlay-apple.css",
                      "~/Content/cd-rsn.css",
                      "~/Content/cd-3dnav.css",
                      "~/Content/Site2.css",
                      "~/Content/UserDefined2.css"
                      //"~/Content/RKD/RKD_Theme.css"
                      // "~/Content/simple-sidebar.css" , "~/Content/materialize/css/materialze.min.css"
                      ));
        }
    }
}
