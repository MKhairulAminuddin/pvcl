﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace xDC_Web.Controllers
{
    [Authorize]
    public class SystemController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult SystemInformation()
        {
            return View();
        }

    }
}