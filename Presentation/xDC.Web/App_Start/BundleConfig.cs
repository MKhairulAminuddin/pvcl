using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Optimization;

namespace xDC_Web {

    public class BundleConfig {

        public static void RegisterBundles(BundleCollection bundles) {

            #region Main Layout

            var scriptBundle = new ScriptBundle("~/Scripts/bundle");
            var styleBundle = new StyleBundle("~/Content/bundle");

            scriptBundle
                .Include(
                    "~/Scripts/jquery-3.5.1.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/underscore.js/underscore.js",
                    "~/Scripts/moment.js/moment.js",
                    "~/Scripts/pace/pace.min.js",
                    "~/Scripts/adminlte.js",
                    "~/Scripts/app.dxGrid.utils.js",
                    "~/Scripts/app.js"
                );


            styleBundle
                .Include(
                    "~/Scripts/pace/themes/blue/pace-theme-flash.css",
                    "~/Content/bootstrap.css",
                    "~/Content/AdminLTE.css",
                    "~/Content/skin-blue.css",
                    "~/Content/font-awesome.css",
                    "~/Content/Site.css"
                );

            bundles.Add(scriptBundle);
            bundles.Add(styleBundle);

            #endregion

            #region AMSD

            #region Inflow Fund Form

            bundles.Add(new ScriptBundle("~/bundles/amsd/inflowFund/edit").Include("~/Scripts/app/amsd/inflowFund/edit.js"));
            bundles.Add(new ScriptBundle("~/bundles/amsd/inflowFund/index").Include("~/Scripts/app/amsd/inflowFund/index.js"));
            bundles.Add(new ScriptBundle("~/bundles/amsd/inflowFund/new").Include("~/Scripts/app/amsd/inflowFund/new.js"));
            bundles.Add(new ScriptBundle("~/bundles/amsd/inflowFund/view").Include("~/Scripts/app/amsd/inflowFund/view.js"));

            #endregion


            #endregion

            #region ISSD

            #region TradeSettlement Form

            bundles.Add(new ScriptBundle("~/bundles/issd/ts/index").Include("~/Scripts/app/issd/tradeSettlement/index.js"));
            bundles.Add(new ScriptBundle("~/bundles/issd/ts/view").Include(
                "~/Scripts/app/issd/tradeSettlement.js",
                "~/Scripts/app/issd/tradeSettlement/view.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/issd/ts/partA/view").Include(
                "~/Scripts/app/issd/tradeSettlement.js",
                "~/Scripts/app/issd/tradeSettlement/partA/view.js"
                ));
            bundles.Add(new ScriptBundle("~/bundles/issd/ts/partB/view").Include(
                "~/Scripts/app/issd/tradeSettlement.js",
                "~/Scripts/app/issd/tradeSettlement/partB/view.js"
                ));
            bundles.Add(new ScriptBundle("~/bundles/issd/ts/partC/view").Include(
                "~/Scripts/app/issd/tradeSettlement.js",
                "~/Scripts/app/issd/tradeSettlement/partC/view.js"
                ));
            bundles.Add(new ScriptBundle("~/bundles/issd/ts/partD/view").Include(
                "~/Scripts/app/issd/tradeSettlement.js",
                "~/Scripts/app/issd/tradeSettlement/partD/view.js"
                ));
            bundles.Add(new ScriptBundle("~/bundles/issd/ts/partE/view").Include(
                "~/Scripts/app/issd/tradeSettlement.js",
                "~/Scripts/app/issd/tradeSettlement/partE/view.js"
                ));
            bundles.Add(new ScriptBundle("~/bundles/issd/ts/partF/view").Include(
                "~/Scripts/app/issd/tradeSettlement.js",
                "~/Scripts/app/issd/tradeSettlement/partF/view.js"
                ));
            bundles.Add(new ScriptBundle("~/bundles/issd/ts/partG/view").Include(
                "~/Scripts/app/issd/tradeSettlement.js",
                "~/Scripts/app/issd/tradeSettlement/partG/view.js"
                ));
            bundles.Add(new ScriptBundle("~/bundles/issd/ts/partH/view").Include(
                "~/Scripts/app/issd/tradeSettlement.js",
                "~/Scripts/app/issd/tradeSettlement/partH/view.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/issd/ts/partA/edit").Include(
                "~/Scripts/app/issd/tradeSettlement.js",
                "~/Scripts/app/issd/tradeSettlement/partA/edit.js"
                ));
            bundles.Add(new ScriptBundle("~/bundles/issd/ts/partB/edit").Include(
                "~/Scripts/app/issd/tradeSettlement.js",
                "~/Scripts/app/issd/tradeSettlement/partB/edit.js"
                ));
            bundles.Add(new ScriptBundle("~/bundles/issd/ts/partC/edit").Include(
                "~/Scripts/app/issd/tradeSettlement.js",
                "~/Scripts/app/issd/tradeSettlement/partC/edit.js"
                ));
            bundles.Add(new ScriptBundle("~/bundles/issd/ts/partD/edit").Include(
                "~/Scripts/app/issd/tradeSettlement.js",
                "~/Scripts/app/issd/tradeSettlement/partD/edit.js"
                ));
            bundles.Add(new ScriptBundle("~/bundles/issd/ts/partE/edit").Include(
                "~/Scripts/app/issd/tradeSettlement.js",
                "~/Scripts/app/issd/tradeSettlement/partE/edit.js"
                ));
            bundles.Add(new ScriptBundle("~/bundles/issd/ts/partF/edit").Include(
                "~/Scripts/app/issd/tradeSettlement.js",
                "~/Scripts/app/issd/tradeSettlement/partF/edit.js"
                ));
            bundles.Add(new ScriptBundle("~/bundles/issd/ts/partG/edit").Include(
                "~/Scripts/app/issd/tradeSettlement.js",
                "~/Scripts/app/issd/tradeSettlement/partG/edit.js"
                ));
            bundles.Add(new ScriptBundle("~/bundles/issd/ts/partH/edit").Include(
                "~/Scripts/app/issd/tradeSettlement.js",
                "~/Scripts/app/issd/tradeSettlement/partH/edit.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/issd/ts/partA/new").Include(
                "~/Scripts/app/issd/tradeSettlement.js",
                "~/Scripts/app/issd/tradeSettlement/partA/new.js"
                ));
            bundles.Add(new ScriptBundle("~/bundles/issd/ts/partB/new").Include(
                "~/Scripts/app/issd/tradeSettlement.js",
                "~/Scripts/app/issd/tradeSettlement/partB/new.js"
                ));
            bundles.Add(new ScriptBundle("~/bundles/issd/ts/partC/new").Include(
                "~/Scripts/app/issd/tradeSettlement.js",
                "~/Scripts/app/issd/tradeSettlement/partC/new.js"
                ));
            bundles.Add(new ScriptBundle("~/bundles/issd/ts/partD/new").Include(
                "~/Scripts/app/issd/tradeSettlement.js",
                "~/Scripts/app/issd/tradeSettlement/partD/new.js"
                ));
            bundles.Add(new ScriptBundle("~/bundles/issd/ts/partE/new").Include(
                "~/Scripts/app/issd/tradeSettlement.js",
                "~/Scripts/app/issd/tradeSettlement/partE/new.js"
                ));
            bundles.Add(new ScriptBundle("~/bundles/issd/ts/partF/new").Include(
                "~/Scripts/app/issd/tradeSettlement.js",
                "~/Scripts/app/issd/tradeSettlement/partF/new.js"
                ));
            bundles.Add(new ScriptBundle("~/bundles/issd/ts/partG/new").Include(
                "~/Scripts/app/issd/tradeSettlement.js",
                "~/Scripts/app/issd/tradeSettlement/partG/new.js"
                ));
            bundles.Add(new ScriptBundle("~/bundles/issd/ts/partH/new").Include(
                "~/Scripts/app/issd/tradeSettlement.js",
                "~/Scripts/app/issd/tradeSettlement/partH/new.js"
                ));

            #endregion

            #region FCA Tagging

            bundles.Add(new ScriptBundle("~/bundles/issd/fcaTagging/index").Include("~/Scripts/app/issd/fcaTagging/index.js"));
            bundles.Add(new ScriptBundle("~/bundles/issd/fcaTagging/view").Include("~/Scripts/app/issd/fcaTagging/view.js"));
            bundles.Add(new ScriptBundle("~/bundles/issd/fcaTagging/edit").Include("~/Scripts/app/issd/fcaTagging/edit.js"));


            #endregion

            #endregion

            #region FID

            #region Treasury Form

            bundles.Add(new ScriptBundle("~/bundles/fid/treasury/index").Include("~/Scripts/app/fid/treasury/index.js"));
            bundles.Add(new ScriptBundle("~/bundles/fid/treasury/view").Include(
                "~/Scripts/app/fid/treasury/treasury.js",
                "~/Scripts/app/fid/treasury/view.js"));
            bundles.Add(new ScriptBundle("~/bundles/fid/treasury/edit").Include(
                "~/Scripts/app/fid/treasury/treasury.js",
                "~/Scripts/app/fid/treasury/edit.js"));
            bundles.Add(new ScriptBundle("~/bundles/fid/treasury/new").Include(
                "~/Scripts/app/fid/treasury/treasury.js",
                "~/Scripts/app/fid/treasury/new.js"));

            #endregion

            #region FCA Tagging

            bundles.Add(new ScriptBundle("~/bundles/fid/fcaTagging/index").Include("~/Scripts/app/fid/fcaTagging/index.js"));
            bundles.Add(new ScriptBundle("~/bundles/fid/fcaTagging/view").Include("~/Scripts/app/fid/fcaTagging/view.js"));
            bundles.Add(new ScriptBundle("~/bundles/fid/fcaTagging/edit").Include("~/Scripts/app/fid/fcaTagging/edit.js"));

            #endregion

            #endregion

            #region Reports

            bundles.Add(new ScriptBundle("~/bundles/report/DealCutOff/FCY").Include("~/Scripts/app/report/dealCutOffFcy.js"));
            bundles.Add(new ScriptBundle("~/bundles/report/DealCutOff/MYR").Include("~/Scripts/app/report/dealCutOffMyr.js"));
            bundles.Add(new ScriptBundle("~/bundles/report/DealCutOff/10AM").Include("~/Scripts/app/report/tenAmDealCutOff.js"));


            #endregion

            #region Administration

            bundles.Add(new ScriptBundle("~/bundles/admin/appConfig").Include("~/Scripts/app/admin/applicationConfig.js"));
            bundles.Add(new ScriptBundle("~/bundles/admin/roleMgmt").Include("~/Scripts/app/admin/roleManagement.js"));
            bundles.Add(new ScriptBundle("~/bundles/admin/userMgmt").Include("~/Scripts/app/admin/userManagement.js"));
            bundles.Add(new ScriptBundle("~/bundles/admin/utility").Include("~/Scripts/app/admin/utility.js"));


            #endregion

            #region Audit

            bundles.Add(new ScriptBundle("~/bundles/audit/form").Include("~/Scripts/app/audit/form.js"));
            bundles.Add(new ScriptBundle("~/bundles/audit/roleMgmt").Include("~/Scripts/app/audit/roleManagement.js"));
            bundles.Add(new ScriptBundle("~/bundles/audit/userAccess").Include("~/Scripts/app/audit/userAccess.js"));
            bundles.Add(new ScriptBundle("~/bundles/audit/userMgmt").Include("~/Scripts/app/audit/userManagement.js"));

            #endregion

            #region Settings

            bundles.Add(new ScriptBundle("~/bundles/setting/approverMgmt").Include("~/Scripts/app/setting/approverManagement.js"));
            bundles.Add(new ScriptBundle("~/bundles/setting/dropdownConfig").Include("~/Scripts/app/setting/dropdownConfig.js"));
            bundles.Add(new ScriptBundle("~/bundles/setting/emailNotificationConfig").Include("~/Scripts/app/setting/emailNotiConfig.js"));


            #endregion

            #region User Profile



            #endregion


#if !DEBUG
            BundleTable.EnableOptimizations = true;
#endif
        }
    }
}