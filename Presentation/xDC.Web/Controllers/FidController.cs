using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using xDC.Infrastructure.Application;
using xDC.Utils;
using xDC_Web.ViewModels.Fid;
using xDC_Web.ViewModels.Fid.Treasury;

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

        #region Treasury

        public ActionResult Treasury()
        {
            return View("MoneyMarketInstrument/Index");
        }

        public ActionResult TreasuryNew()
        {
            var model = new TreasuryFormVM
            {
                FormStatus = Common.FormStatus.Draft,
                PreparedBy = User.Identity.Name,
                PreparedDate = DateTime.Now,
            };
            return View("MoneyMarketInstrument/New", model);
        }

        public ActionResult TreasuryEdit()
        {
            return View("MoneyMarketInstrument/Index");
        }

        public ActionResult TreasuryView()
        {
            return View("MoneyMarketInstrument/Index");
        }

        #endregion


    }
}