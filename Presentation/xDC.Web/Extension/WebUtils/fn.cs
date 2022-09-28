using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xDC_Web.Extension.WebUtils
{
    public static class fn
    {
        public static string GetClientBrowser(string userAgent)
        {
            var ua = UAParser.Parser.GetDefault();
            var client = ua.Parse(userAgent);

            var finalString = $"{client.UA.Family} {client.UA.Major}.{client.UA.Minor}.{client.UA.Patch}";

            return finalString;
        }

        public static string GetIPAddress()
        {
            System.Web.HttpContext context = System.Web.HttpContext.Current;
            string ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                {
                    return addresses[0];
                }
            }

            return context.Request.ServerVariables["REMOTE_ADDR"];
        }
    }
}