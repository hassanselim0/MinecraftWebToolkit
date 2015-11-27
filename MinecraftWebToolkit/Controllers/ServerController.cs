using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FileIO = System.IO.File;
using WebConfig = System.Web.Configuration.WebConfigurationManager;

namespace MinecraftWebToolkit.Controllers
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
            try
            {
                ProcHttpClient.StartProc("McServer", WebConfig.AppSettings["JrePath"],
                    "-Xmx1024M -Xms512M -jar " + WebConfig.AppSettings["McJarFile"] + " nogui",
                    WebConfig.AppSettings["McServerPath"]);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Content(ex.ToString());
            }

            return Content("OK " + DateTime.Now.Ticks);
        }

        public ActionResult SendCommand(string command)
        {
            ProcHttpClient.WriteLine("McServer", command);

            return Content("OK " + DateTime.Now.Ticks);
        }

        public ActionResult GetOutput(long since = 0)
        {
            var log = ProcHttpClient.GetLog("McServer", since)
                .Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Split(new[] { '|' }, 2));

            var lastTimestamp = log.Last()[0];

            var result = new
            {
                Output = log.Aggregate("", (s1, s2) =>
                    s1 + HttpUtility.HtmlEncode(s2[1]) + "<br />\r\n"),
                LastTimestamp = lastTimestamp,
                Ticks = DateTime.Now.Ticks,
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public ActionResult Map(string world)
        {
            var serverPath = WebConfig.AppSettings["McServerPath"];

            ViewBag.Worlds =
                Directory.EnumerateDirectories(serverPath)
                .Where(d => FileIO.Exists(d + "\\level.dat"))
                .Select(d => Path.GetFileName(d))
                .ToList();

            ViewBag.SelWorld = world ?? McProperties.GetValue("level-name");

            return View();
        }

        public ActionResult StartMapper(string world)
        {
            var serverPath = WebConfig.AppSettings["McServerPath"];
            if (world == null) world = McProperties.GetValue("level-name");

            var mapsDir = Path.Combine(serverPath, "Map");
            if (!Directory.Exists(mapsDir))
                Directory.CreateDirectory(mapsDir);

            try
            {
                ProcHttpClient.StartProc("Mapper",
                    Path.Combine(serverPath, "Overviewer\\overviewer.exe"),
                    "-p 4 --rendermodes=smooth_lighting,smooth_night \""
                    + world + "\" \"" + Path.Combine("Map", world) + "\"",
                    serverPath);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Content(ex.ToString());
            }

            return Content("OK " + DateTime.Now.Ticks);
        }

        public ActionResult MapperProgress()
        {
            var log = ProcHttpClient.GetLog("Mapper", 0)
                .Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Split(new[] { '|' }, 2));

            if (!log.Any())
                return Content("");

            if (log.Last()[1] == "Process Ended!")
            {
                ProcHttpClient.ClearLog("Mapper");
                return Content("Completed");
            }

            var lastProg = log.LastOrDefault(s => s[1].Contains("% complete"));

            if (lastProg == null)
                return Content("0%");

            return Content(lastProg[1].Split(' ')[8]);
        }

        public ActionResult Properties()
        {
            return View(McProperties.GetAll());
        }

        [HttpPost]
        public ActionResult Properties(List<McProperty> props)
        {
            props.RemoveAll(p => string.IsNullOrEmpty(p.Name));

            McProperties.SetAll(props);

            return RedirectToAction("Properties");
        }

        public ActionResult RestoreProperties()
        {
            McProperties.RestoreBackup();

            return RedirectToAction("Properties");
        }
    }
}
