using System.Web;
using System.Web.Mvc;
using xDC_Web.Extension.CustomAttribute;

namespace xDC_Web {
    public class FilterConfig {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters) 
        {
            filters.Add(new HandleErrorAttribute());
            //filters.Add(new SqlExceptionFilter());
        }
    }
}
