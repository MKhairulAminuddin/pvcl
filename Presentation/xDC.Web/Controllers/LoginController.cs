using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;
using xDC.Services.Membership;
using xDC_Web.Extension;
using xDC_Web.Models;

namespace xDC_Web.Controllers
{
    public class LoginController : Controller
    {
        [AllowAnonymous]
        public virtual ActionResult Index(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Index(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // usually this will be injected via DI. but creating this manually now for brevity
            IAuthenticationManager authenticationManager = HttpContext.GetOwinContext().Authentication;
            var authService = new AdAuthentication(authenticationManager);

            // get client browser
            model.ClientBrowser = Extension.WebUtils.fn.GetClientBrowser(Request.Headers["User-Agent"]);
            model.IpAddress = Extension.WebUtils.fn.GetIPAddress();
            var authenticationResult = authService.SignIn(model);

            if (authenticationResult.IsSuccess)
            {
                // we are in!
                return RedirectToLocal(returnUrl);
            }

            ModelState.AddModelError("", authenticationResult.ErrorMessage);

            return View(model);
        }

        [HandleAntiForgeryError]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Logoff()
        {
            IAuthenticationManager authenticationManager = HttpContext.GetOwinContext().Authentication;
            authenticationManager.SignOut(xDcAuth.ApplicationCookie);
            Session.Abandon();

            return RedirectToAction("Index");
        }


        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
    }
}