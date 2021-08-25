using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using xDC.Infrastructure.Application;
using xDC_Web.ViewModels.Fid;

namespace xDC_Web.Controllers
{
    [Authorize(Roles = "Administrator, Power User, FID")]
    [RoutePrefix("fid")]
    public class FidController : Controller
    {
        // GET: Fid
        public ActionResult Index()
        {
            return View();
        }

        #region 10 AM Cut Off

        [Route("Summary")]
        public ActionResult Summary()
        {
            return View("TenAmCutOff/Summary");
        }

        [Route("FcaTagging")]
        public ActionResult FcaTagging()
        {
            return View("TenAmCutOff/FcaTagging");
        }

        [Route("FcaTagging/Edit/{formId}")]
        public ActionResult FcaTaggingEdit(int formId)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var form = db.FID_TS10.FirstOrDefault(x => x.Id == formId);

                    if (form != null)
                    {
                        var vModel = new EditFcaAccountAssignmentVM();
                        vModel.Currency = form.Currency;
                        if (form.SettlementDate != null)
                        {
                            vModel.SettlementDate = form.SettlementDate.Value;
                        }

                        return View("TenAmCutOff/FcaTaggingEdit", vModel);

                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Form Not found";
                        return View("Error");
                    }
                }

            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Error");
            }
        }

        #endregion

        #region Money Market

        public ActionResult Mmi()
        {
            return View("MoneyMarketInstrument/Index");
        }

        public ActionResult NewMmi()
        {
            return View("MoneyMarketInstrument/New");
        }

        public ActionResult EditMmi()
        {
            return View("MoneyMarketInstrument/Index");
        }

        public ActionResult ViewMmi()
        {
            return View("MoneyMarketInstrument/Index");
        }

        #endregion


    }
}