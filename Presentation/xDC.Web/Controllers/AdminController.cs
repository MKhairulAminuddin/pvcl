using DevExpress.Data.ODataLinq.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using xDC.Infrastructure.Application;
using xDC_Web.Extension.CustomAttribute;
using xDC_Web.ViewModels.Admin;

namespace xDC_Web.Controllers
{
    [Authorize(Roles = "Administrator")]
    [KashflowAuthorize(PermissionName = xDC.Utils.Common.PermissionKey.Administration)]
    public class AdminController : Controller
    {
        public ActionResult UserManagement()
        {
            var model = new UserManagementVM();

            using (var db = new kashflowDBEntities())
            {
                var users = db.AspNetUsers;
                var roles = db.AspNetRoles;
                foreach (var item in users.GroupBy(x => x.Locked))
                {
                    model.TotalRegisteredUsers.Add(new UserManagementVMKeyValue()
                    {
                        Key = (item.Key == true) ? "Inactive" : "Active",
                        Value = item.Count()
                    });
                }
                foreach (var item in users.GroupBy(x => x.Department))
                {
                    model.TotalDepartmentBreakdown.Add(new UserManagementVMKeyValue()
                    {
                        Key = item.Key,
                        Value = item.Count()
                    });
                }
                foreach (var item in roles)
                {
                    model.TotalUserGroupBreakdown.Add(new UserManagementVMKeyValue()
                    {
                        Key = item.Name,
                        Value = item.AspNetUsers.Count
                    });
                }
            }


            return View(model);
        }

        public ActionResult RoleManagement()
        {
            return View();
        }

        public ActionResult ApplicationConfig()
        {
            return View();
        }

        public ActionResult SystemLogs()
        {
            return View();
        }

        public ActionResult SystemInformation()
        {
            return View();
        }

        public ActionResult Utility()
        {
            return View();
        }

        public ActionResult UserAccessLog()
        {
            return View();
        }

        public ActionResult UserActivityLog()
        {
            return View();
        }

        public ActionResult AdminLog()
        {
            return View();
        }

    }
}