using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Http.Gateway.Admin.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Modules()
        {
            return View();
        }

        public ActionResult Registries()
        {
            return View();
        }

        public ActionResult Services()
        {
            return View();
        }
    }
}