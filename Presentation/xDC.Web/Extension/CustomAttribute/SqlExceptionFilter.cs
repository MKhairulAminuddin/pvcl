
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace xDC_Web.Extension.CustomAttribute
{
    public class SqlExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext filterContext)
        {
            filterContext.Result = new RedirectResult("~/Base/NoDbConnection");
        }
    }
}