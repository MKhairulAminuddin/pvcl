using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http.Results;
using System.Web.Mvc;
using Newtonsoft.Json;
using xDC_Web.Models;

namespace xDC_Web.Controllers
{
    [Authorize(Roles = "Administrator, Power User, Amsd")]
    public class AmsdController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult NewInflowFundsForm()
        {
            return View();
        }

        public ActionResult ViewInflowFundsForm(string id)
        {
            return View();
        }
    }
}