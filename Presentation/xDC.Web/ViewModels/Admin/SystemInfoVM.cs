using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xDC_Web.ViewModels.Admin
{
    public class SystemInfoVM
    {
        public string ServerIp { get; set; }
        public string ServerName { get; set; }
        public string NetworkName { get; set; }
        public string ServiceAccountName { get; set; }
        public string OsVersion { get; set; }
        public string OsName { get; set; }
        public string UpTime { get; set; }
        public string ServerTime { get; set; }
        public string IisVersion { get; set; }
        public string SslEnable { get; set; }
        public string PathInfo { get; set; }
        public string PathTranslated { get; set; }
        public string ServerPort { get; set; }
        public string SessionId { get; set; }
        public List<ServerDriveInfoVM> Drives { get; set; }
        public string MemorySize { get; set; }

        public SystemInfoVM()
        {
            Drives = new List<ServerDriveInfoVM>();
        }
    }

    public class ServerDriveInfoVM
    {
        public string DriveName { get; set; }
        public string DriveTotalSize { get; set; }
        public string DriveTotalFreeSize { get; set; }
        public string Percentage { get; set; }
    }

    public class ServerMemoryInfoVM
    {
        public string TotalMemorySize { get; set; }
        public string TotalMemoryFree { get; set; }
    }
}