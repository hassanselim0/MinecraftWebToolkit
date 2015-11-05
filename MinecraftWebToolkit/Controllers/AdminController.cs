using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Cache;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;

namespace MinecraftWebToolkit.Controllers
{
    [NoCache]
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        public ActionResult Index()
        {
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
            // http://assets.minecraft.net/ <- This is an XML file
            // http://assets.minecraft.net/V_E_R/minecraft_server.jar
            // Old Stuff, from beta 1.8 pre till 1.5.2, and from 11w47 till 13w12 snapshots

            // https://s3.amazonaws.com/Minecraft.Download/versions/versions.json
            // https://s3.amazonaws.com/Minecraft.Download/versions/V.E.R/minecraft_server.V.E.R.jar
            // Minimum Available Server Version: 1.2.5

            var client = new System.Net.WebClient();
            client.CachePolicy = new RequestCachePolicy(RequestCacheLevel.CacheIfAvailable);

            HttpContext.Application["UpdateProgress"] = "Getting Latest Version...";

            // Get Latest Version
            var jObj = JObject.Parse(client.DownloadString(
                "https://s3.amazonaws.com/Minecraft.Download/versions/versions.json"));
            var ver = jObj["latest"]["release"].Value<string>();

            var jarFile = "minecraft_server." + ver + ".jar";
            var jarUri = "https://s3.amazonaws.com/Minecraft.Download/versions/" + ver + "/" + jarFile;

            var config = WebConfigurationManager.OpenWebConfiguration("~");

            client.DownloadProgressChanged += (o, e) =>
                HttpContext.Application["UpdateProgress"] = e.ProgressPercentage + "%";

            client.DownloadFileCompleted += (o, e) =>
            {
                if (e.Error != null)
                {
                    HttpContext.Application["UpdateProgress"] = "Error: " + e.Error;
                    return;
                }

                HttpContext.Application["UpdateProgress"] = "Completed";

                config.AppSettings.Settings["McJarFile"].Value = jarFile;
                config.Save();
            };

            HttpContext.Application["UpdateProgress"] = "0%";
            System.Threading.Tasks.Task.Run(() => // Workaround to allow Async call
            {
                try
                {
                    client.DownloadFileAsync(new Uri(jarUri),
                        config.AppSettings.Settings["McServerPath"].Value + jarFile);
                }
                catch (Exception ex)
                {
                    HttpContext.Application["UpdateProgress"] = "Error: " + ex;
                }
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
