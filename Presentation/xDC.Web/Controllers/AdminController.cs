using DevExpress.Data.ODataLinq.Helpers;
using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using xDC.Infrastructure.Application;
using xDC_Web.Extension.CustomAttribute;
using xDC_Web.ViewModels.Admin;

namespace xDC_Web.Controllers
{
    [Authorize]
    [KflowAuthorize(xDC.Utils.Common.PermissionKey.Administration)]
    public class AdminController : Controller
    {
        [KflowAuthorize(xDC.Utils.Common.PermissionKey.Administration_UserManagement)]
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

        [KflowAuthorize(xDC.Utils.Common.PermissionKey.Administration_RolesManagement)]
        public ActionResult RoleManagement()
        {
            return View();
        }

        [KflowAuthorize(xDC.Utils.Common.PermissionKey.Administration_ApplicationConfig)]
        public ActionResult ApplicationConfig()
        {
            return View();
        }

        [KflowAuthorize(xDC.Utils.Common.PermissionKey.Administration_SystemInformation)]
        public ActionResult SystemInformation()
        {
            var serverDrives = DriveInfo.GetDrives();
            var drives = new List<ServerDriveInfoVM>();
            foreach (var item in serverDrives)
            {
                if (item.IsReady)
                {
                    var drive = new ServerDriveInfoVM()
                    {
                        DriveName = item.Name,
                        DriveTotalSize = xDC.Utils.Common.FormatBytes(item.TotalSize, true),
                        DriveTotalFreeSize = xDC.Utils.Common.FormatBytes(item.TotalFreeSpace, true),
                        Percentage = $"{100 - ((item.AvailableFreeSpace / (float)item.TotalSize) * 100):0.00}%"
                        
                    };
                    drives.Add(drive);
                }
            }

            var model = new SystemInfoVM()
            {
                ServerIp = Request.ServerVariables["LOCAL_ADDR"],
                ServerName = Environment.MachineName,
                NetworkName = Environment.UserDomainName,
                ServiceAccountName = Environment.UserName,
                OsVersion = Environment.OSVersion.ToString(),
                OsName = new ComputerInfo().OSFullName,
                UpTime = (Environment.TickCount / (1000 * 60 * 60)) + " Hours",
                ServerTime = DateTime.Now.ToLongDateString(),
                IisVersion = Request.ServerVariables["SERVER_SOFTWARE"],
                SslEnable = Request.ServerVariables["HTTPS"],
                PathInfo = Request.ServerVariables["PATH_INFO"],
                PathTranslated = Request.ServerVariables["PATH_TRANSLATED"],
                ServerPort = Request.ServerVariables["SERVER_PORT"],
                SessionId = Session.SessionID,
                MemorySize = xDC.Utils.Common.FormatBytes((long)new ComputerInfo().TotalPhysicalMemory, true)
            };
            model.Drives.AddRange(drives);

            return View(model);
        }

        [KflowAuthorize(xDC.Utils.Common.PermissionKey.Administration_Utility)]
        public ActionResult Utility()
        {
            return View();
        }


    }
}