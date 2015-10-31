using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace McServerWebsite.Controllers
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
            var user = System.Web.Security.Membership.GetUser();
            if (user != null && user.IsApproved)
            {
                MvcApplication.McServer.UserIPs[user.UserName] = Request.UserHostAddress;
                MvcApplication.McServer.UserLastPing[user.UserName] = DateTime.UtcNow;
            }

            if (HttpContext.Application["UpdateProgress"] != null)
                return Content("Updating");
            else if (MvcApplication.McServer.IsRunning)
                return Content("Online");
            else
                return Content("Offline");
        }
    }
}
