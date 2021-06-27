using System;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;

namespace xDC_Web
{
    public static class xDcAuth
    {
        public const String ApplicationCookie = "xDcAuth";
    }

    public partial class Startup
    {
        public void ConfigureAuth(IAppBuilder app)
        {
            // need to add UserManager into owin, because this is used in cookie invalidation
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = xDcAuth.ApplicationCookie,
                LoginPath = new PathString("/Login"),
                //ReturnUrlParameter = ,

                Provider = new CookieAuthenticationProvider
                {
                    OnApplyRedirect = ctx =>
                    {
                        if (!IsApiRequest(ctx.Request))
                        {
                            ctx.Response.Redirect(ctx.RedirectUri);
                        }
                    }
                },
                CookieName = "xDcAuth",
                CookieHttpOnly = true,
                ExpireTimeSpan = TimeSpan.FromHours(1), // adjust to your needs
            });
        }

        private bool IsApiRequest(IOwinRequest request)
        {
            return request.Uri.AbsolutePath.StartsWith("/api");
        }
    }
}