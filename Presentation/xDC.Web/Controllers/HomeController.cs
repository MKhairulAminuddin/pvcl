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
    public class HomeController : Controller 
    {
        private readonly IIfFormService _ifFormService;
        private readonly ITsFormService _tsFormService;
        private readonly ITreasuryFormService _tFormService;

        public HomeController(ITreasuryFormService tFormService, IIfFormService ifFormService, ITsFormService tsFormService)
        {
            _tFormService = tFormService;
            _ifFormService = ifFormService;
            _tsFormService = tsFormService;
        }

        public ActionResult Index() {

            var model = new HomeDashboard();

            if (MenuSecurity.IsAuthenticatedAndAllowed(Common.PermissionKey.AMSD_InflowFundForm_Edit) || User.IsInRole("Power User"))
            {
                model.IfFormSummary = _ifFormService.IfFormSummaryList();
                model.IfAmountSummary = _ifFormService.IfAmountSummary();
            }

            if (MenuSecurity.IsAuthenticatedAndAllowed(Common.PermissionKey.ISSD_TradeSettlementForm_Edit) || User.IsInRole("Power User"))
            {
                model.TsOpeningBalanceSummary = _tsFormService.OpeningBalanceSummary();
                model.TsFormSummary = _tsFormService.TsFormSummaryList();
            }

            if (MenuSecurity.IsAuthenticatedAndAllowed(Common.PermissionKey.FID_TreasuryForm_Edit) || User.IsInRole("Power User"))
            {
                model.TreasuryFormSummary = _tFormService.TreasuryFormSummaryList();
            }

            return View(model);
        }
    }
}