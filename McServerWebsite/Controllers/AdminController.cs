using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace McServerWebsite.Controllers
{
    [NoCache]
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        public ActionResult Index()
        {
            if (HttpContext.Application["UpdateProgress"] != null)
                ViewData["ServerStatus"] = "Updating";
            else
                ViewData["ServerStatus"] = MvcApplication.McServer.IsRunning ? "Running" : "Stopped";

            return View();
        }

        public ActionResult KillServer()
        {
            MvcApplication.McServer.Kill();

            return RedirectToAction("");
        }

        public ActionResult SetPingIP(string ip)
        {
            System.Configuration.ConfigurationManager.AppSettings["AzurePingIP"] = ip;

            return RedirectToAction("");
        }

        public ActionResult ApproveAccount(string username)
        {
            var user = Membership.GetUser(username);
            user.IsApproved = true;
            Membership.UpdateUser(user);

            return RedirectToAction("");
        }

        public ActionResult AssignRole(string username, string role)
        {
            Roles.AddUserToRole(username, role);

            return RedirectToAction("");
        }

        public ActionResult RetractRole(string username, string role)
        {
            Roles.RemoveUserFromRole(username, role);

            return RedirectToAction("");
        }

        public ActionResult UnlockAccount(string username)
        {
            Membership.GetUser(username).UnlockUser();

            return RedirectToAction("");
        }

        public ActionResult DeleteAccount(string username)
        {
            Membership.DeleteUser(username);

            return RedirectToAction("");
        }

        public ActionResult UpdateServer()
        {
			// https://s3.amazonaws.com/Minecraft.Download/versions/versions.json
			// https://s3.amazonaws.com/Minecraft.Download/versions/" + Ver + "/minecraft_server." + Ver + ".jar
			
            var client = new System.Net.WebClient();

            HttpContext.Application["UpdateProgress"] = "0%";

            client.DownloadProgressChanged += (o, e) =>
            {
                HttpContext.Application["UpdateProgress"] = e.ProgressPercentage + "%";
            };

            client.DownloadFileCompleted += (o, e) =>
            {
                HttpContext.Application["UpdateProgress"] = "Completed";
            };

            System.Threading.Tasks.Task.Run(() =>
            {
                client.DownloadFile(new Uri("http://www.minecraft.net/download/minecraft_server.jar"),
                    System.Configuration.ConfigurationManager.AppSettings["McServerPath"] + "minecraft_server.jar");
                HttpContext.Application["UpdateProgress"] = "Completed";
            });

            return Content("OK " + DateTime.Now.Ticks);
        }

        public ActionResult UpdateProgress()
        {
            if (HttpContext.Application["UpdateProgress"] == null)
                return Content("");

            string progress = HttpContext.Application["UpdateProgress"].ToString();
            if (progress == "Completed")
                HttpContext.Application["UpdateProgress"] = null;

            return Content(progress);
        }
    }
}
