using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using xDC.Domain.Web.HOME;
using xDC.Services.Form;
using xDC.Utils;
using xDC_Web.Extension;

namespace xDC_Web.Controllers
{

    [Authorize]
    public class HomeController : Controller {
        public ActionResult Index() {

            var model = new HomeDashboard();

            if (MenuSecurity.IsAuthenticatedAndAllowed(Common.PermissionKey.AMSD_InflowFundForm_Edit) || User.IsInRole("Power User"))
            {
                model.IfFormSummary = IfFormService.IfFormSummaryList();
                model.IfAmountSummary = IfFormService.IfAmountSummary();
            }

            if (MenuSecurity.IsAuthenticatedAndAllowed(Common.PermissionKey.ISSD_TradeSettlementForm_Edit) || User.IsInRole("Power User"))
            {
                model.TsOpeningBalanceSummary = TsFormService.OpeningBalanceSummary();
                model.TsFormSummary = TsFormService.TsFormSummaryList();
            }

            if (MenuSecurity.IsAuthenticatedAndAllowed(Common.PermissionKey.FID_TreasuryForm_Edit) || User.IsInRole("Power User"))
            {
                model.TreasuryFormSummary = TreasuryFormService.TreasuryFormSummaryList();
            }

            return View(model);
        }
    }
}