using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Utils;
using xDC_Web.Models;
using xDC_Web.ViewModels;

namespace xDC_Web.Controllers
{
    [Authorize(Roles = "Administrator, Power User, IISD")]
    public class IisdController : BaseController
    {
        public ActionResult Index()
        {
            using (var db = new kashflowDBEntities())
            {
                var isApprover = db.Config_Approver.Any(x => x.Username == User.Identity.Name);
                var isIisdUser = User.IsInRole(Config.AclIisd);

                var model = new IisdLandingPageViewModel()
                {
                    IsAllowedToCreateForm = (!isApprover && isIisdUser)
                };
                
                return View(model);
            }

            
        }

        [Authorize(Roles = "IISD")]
        public ActionResult NewTradeSettlement()
        {
            var model = new TradeSettlementFormViewModel()
            {
                PreparedBy = User.Identity.Name,
                PreparedDate = DateTime.Now,
                FormStatus = Common.FormStatusMapping(1),
                IsDraftEnabled = true
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
                        var formObj = new TradeSettlementFormViewModel()
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