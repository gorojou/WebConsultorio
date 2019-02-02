using System.Web.Optimization;

namespace WebMedico
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/md5").Include(
                      "~/Scripts/spark-md5.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                     "~/Content/Assets/js/bootstrap.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/session").Include(
                        "~/Scripts/bootstrap-session-timeout.js",
                        "~/Scripts/utility.js"));
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Content/Assets/js/jquery-3.1.1.min.js",
                        "~/Content/Assets/js/jquerysession.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Content/Assets/js/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/customval").Include(
                        "~/Content/Assets/js/jquery.validate.js",
                        "~/Content/Assets/js/jquery.validate.custom.js"));

            bundles.Add(new ScriptBundle("~/bundles/mdb").Include(
                      "~/Content/Assets/js/mdb.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/imageviewer").Include(
                      "~/Content/Assets/js/viewer.min.js"));

            bundles.Add(new StyleBundle("~/css/imageviewer").Include(
          "~/Content/Assets/css/viewer.min.css"));

            bundles.Add(new ScriptBundle("~/bundles/tether").Include(
                      "~/Content/Assets/js/tether.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/ajax").Include(
                        "~/Content/Assets/js/jquery.unobtrusive-ajax.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/charts").Include(
                        "~/Content/Assets/js/Chart.bundle.min.js"));

            bundles.Add(new StyleBundle("~/css/css").Include(
                      "~/Content/Assets/css/font-awesome.min.css",
                      "~/Content/Assets/css/material-icons.css",
                      "~/Content/Assets/css/bootstrap.min.css",
                      "~/Content/Assets/css/mdb.min.css",
                      "~/Content/Assets/css/mdbfix.css",
                      "~/Content/Assets/css/mdb.custom-components.css",
                      "~/Content/Assets/css/style.css",
                      "~/Content/loader-default.css"
                     ));
        }
    }
}