using System.Web.Mvc;
using System.Web.Routing;

public class HandleAntiForgeryError : ActionFilterAttribute, IExceptionFilter
{
    #region IExceptionFilter Members

    public void OnException(ExceptionContext filterContext)
    {
        var exception = filterContext.Exception as HttpAntiForgeryException;
        if (exception != null)
        {
            var routeValues = new RouteValueDictionary();
            routeValues["controller"] = "Login";
            routeValues["action"] = "Index";
            filterContext.Result = new RedirectToRouteResult(routeValues);
            filterContext.ExceptionHandled = true;
        }
    }

    #endregion
}