using System;
using System.Linq;
using System.Web.Mvc;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Utils;
using xDC_Web.Extension.DocGenerator;
using xDC_Web.Models;
using xDC_Web.Models.MailMerge;

namespace xDC_Web.Controllers
{
    [Authorize(Roles = "Administrator, Power User, Amsd")]
    public class AmsdController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult NewInflowFundsForm()
        {
            var model = new ViewInflowFundStatusForm()
            {
                PreparedBy = User.Identity.Name,
                FormStatus = Common.FormStatusMapping(1)
            };

            return View(model);
        }

        public ActionResult EditInflowFundsForm(string id)
        {
            using (var db = new kashflowDBEntities())
            {
                var formId = Convert.ToInt32(id);
                var getForm = db.FormHeader.FirstOrDefault(x => x.Id == formId);

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

                        ApprovePermission = false // to ganti with workflow checking
                    };
                    return View("NewInflowFundsForm", formObj);
                }
                else
                {
                    return HttpNotFound();
                }
            }

            
        }

        public ActionResult InflowFundsFormStatus(string id)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var formId = Convert.ToInt32(id);
                    var getForm = db.FormHeader.FirstOrDefault(x => x.Id == formId);

                    if (getForm!= null)
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
                return HttpNotFound();
            }
        }
        
        public ActionResult ViewInflowFundsForm(string id)
        {
            try
            {
                var formId = Convert.ToInt32(id);
                AmsdInflowFundFormPreviewModel model = new AmsdInflowFundFormPreviewModel();
                model.Id = formId;

                return View(model);
            }
            catch (Exception ex)
            {
                return HttpNotFound();
            }
            
        }


        // Print Form
        [HttpPost]
        public ActionResult PrintInflowFund(string id)
        {
            try
            {
                var formId = Convert.ToInt32(id);

                var generatedDocumentFile = new InflowFundsFormDoc().GenerateExcelFile(formId);

                if (!string.IsNullOrEmpty(generatedDocumentFile))
                {
                    return Content(generatedDocumentFile);
                }
                else
                {
                    return HttpNotFound();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return HttpNotFound();
            }
        }
        
        public ActionResult GetPrintInflowFund(string id)
        {
            try
            {
                var fileStream = new DocGeneratorBase().GetFile(id);

                if (fileStream != null)
                {
                    var fileNameOnClient = id + ".xlsx";
                    Response.AddHeader("Content-Disposition", "attachment; filename=" + fileNameOnClient);
                    return File(fileStream, Common.ConvertIndexToContentType(4));
                }
                else
                {
                    return HttpNotFound();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return HttpNotFound();
            }
        }
    }
}