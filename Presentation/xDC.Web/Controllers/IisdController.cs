﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace xDC_Web.Controllers
{
    [Authorize(Roles = "Administrator, Power User, IISD")]
    public class IisdController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult NewTradeSettlement()
        {
            return View();
        }
    }
}