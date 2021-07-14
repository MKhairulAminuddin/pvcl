using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Optimization;

namespace xDC_Web {

    public class BundleConfig {

        public static void RegisterBundles(BundleCollection bundles) {

            var scriptBundle = new ScriptBundle("~/Scripts/bundle");
            var styleBundle = new StyleBundle("~/Content/bundle");
            
            scriptBundle
                .Include(
                    "~/Scripts/jquery-3.5.1.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/underscore.js/underscore.js",
                    "~/Scripts/moment.js/moment.js",
                    "~/Scripts/adminlte.js",
                    "~/Scripts/app.dxGrid.utils.js",
                    "~/Scripts/app.js"
                );

            
            styleBundle
                .Include(
                    "~/Content/bootstrap.css",
                    "~/Content/AdminLTE.css",
                    "~/Content/skin-blue.css",
                    "~/Content/font-awesome.css",
                    "~/Content/Site.css"
                );

            bundles.Add(scriptBundle);
            bundles.Add(styleBundle);

#if !DEBUG
            BundleTable.EnableOptimizations = true;
#endif
        }
    }
}