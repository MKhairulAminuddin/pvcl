using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace xDC_Web.Controllers
{
    [Authorize(Roles = "Administrator, Power User, FID")]
    [RoutePrefix("fid")]
    public class FidController : Controller
    {
        // GET: Fid
        public ActionResult Index()
        {
            return View();
        }

        [Route("10amCutOff")]
        public ActionResult TenAmCutOff()
        {
            return View();
        }

        [Route("FcaAccountAssignment")]
        public ActionResult FcaAccountAssignment()
        {
            return View();
        }

        [Route("FcaAccountAssignment/Edit")]
        public ActionResult EditFcaAccountAssignment()
        {
            return View();
        }
    }
}