﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace xDC_Web.Controllers
{
    [Authorize(Roles = "Administrator, Power User, Amsd")]
    public class AmsdController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult NewForm()
        {
            return View();
        }

        public ActionResult ApproveForm()
        {
            return View();
        }
    }
}