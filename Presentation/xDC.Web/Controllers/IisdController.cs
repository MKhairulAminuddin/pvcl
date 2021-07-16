using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Utils;
using xDC_Web.Models;

namespace xDC_Web.Controllers
{
    [Authorize(Roles = "Administrator, Power User, IISD")]
    public class IisdController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult NewTradeSettlement()
        {
            var model = new ViewTradeSettlementForm()
            {
                PreparedBy = User.Identity.Name,
                FormStatus = Common.FormStatusMapping(1)
            };

            return View(model);
        }

        public ActionResult TradeSettlementFormStatus(string id)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var formId = Convert.ToInt32(id);
                    var getForm = db.Form_Header.FirstOrDefault(x => x.Id == formId);

                    if (getForm != null)
                    {
                        var formObj = new ViewInflowFundStatusForm()
                        {
                            Id = getForm.Id,
                            PreparedBy = getForm.PreparedBy,
                            PreparedDate = getForm.PreparedDate,
                            ApprovedBy = getForm.ApprovedBy,
                            ApprovedDate = getForm.ApprovedDate,
                            FormStatus = getForm.FormStatus,

                            ApprovePermission = getForm.ApprovedBy == User.Identity.Name
                        };
                        return View(formObj);
                    }
                }
                return HttpNotFound();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return HttpNotFound();
            }
        }
    }
}