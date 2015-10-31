using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace McServerWebsite.Controllers
{
    [NoCache, Authorize(Roles = "Moderator")]
    public class ServerController : Controller
    {
        public ActionResult Console()
        {
            return View();
        }

        public ActionResult Start()
        {
            MvcApplication.McServer.Start();

            return Content("OK " + DateTime.Now.Ticks);
        }

        public ActionResult SendCommand(string command)
        {
            MvcApplication.McServer.SendCommand(command);

            return Content("OK " + DateTime.Now.Ticks);
        }

        public ActionResult GetOutput(string since)
        {
            if (MvcApplication.McServer.ConsoleHistory.Count == 0)
                return Json(new { Output = "", LastMessage = 0 }, JsonRequestBehavior.AllowGet);

            int startAfter;
            if (since == "") startAfter = MvcApplication.McServer.ConsoleHistory.Count - 101;
            else startAfter = int.Parse(since);

            var result = new
            {
                Output = MvcApplication.McServer.ConsoleHistory.SkipWhile((cm, i) => i <= startAfter)
                    .Aggregate("", (s1, s2) => s1 + HttpUtility.HtmlEncode(s2) + "<br />\r\n"),
                LastMessage = MvcApplication.McServer.ConsoleHistory.Count - 1,
                Ticks = DateTime.Now.Ticks,
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public ActionResult Map()
        {
            return View();
        }

        public ActionResult StartMapper()
        {
            Mapper.Start();

            return Content("OK " + DateTime.Now.Ticks);
        }

        public ActionResult MapperProgress()
        {
            var prog = Mapper.Progress;
            if (prog == "Completed") Mapper.Progress = "";

            return Content(prog);
        }

        public ActionResult Properties()
        {
            var props = System.IO.File.ReadAllText(
                ConfigurationManager.AppSettings["McServerPath"] + "server.properties");

            return View((object)props);
        }

        [HttpPost]
        public ActionResult Properties(string props)
        {
            var path = ConfigurationManager.AppSettings["McServerPath"];

            System.IO.File.Move(path + "server.properties", path + "server.properties-backup");

            System.IO.File.WriteAllText(path + "server.properties", props);

            return RedirectToAction("Properties");
        }

        public ActionResult RestoreProperties()
        {
            var path = ConfigurationManager.AppSettings["McServerPath"];

            System.IO.File.Copy(path + "server.properties-backup", path + "server.properties", true);

            return RedirectToAction("Properties");
        }
    }
}
