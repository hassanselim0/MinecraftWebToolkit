using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinecraftWebToolkit.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [NoCache]
        public ActionResult Ping()
        {
            return Content(DateTime.Now.Ticks.ToString());
        }

        [NoCache]
        public ActionResult ServerStatus()
        {
            McAuthHttpClient.Renew(User.Identity.Name, Request.UserHostAddress);

            if (HttpContext.Application["UpdateProgress"] != null)
                return Content("Updating");
            else if (ProcHttpClient.IsRunning("McServer"))
                return Content("Online");
            else
                return Content("Offline");
        }
    }
}
