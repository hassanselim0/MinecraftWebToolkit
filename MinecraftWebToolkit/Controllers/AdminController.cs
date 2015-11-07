using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Cache;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using WebConfig = System.Web.Configuration.WebConfigurationManager;

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
            McServer.Inst.Kill();

            return RedirectToAction("");
        }

        public ActionResult SetPingIP(string ip)
        {
            WebConfig.AppSettings["AzurePingIP"] = ip;

            return RedirectToAction("");
        }

        // User Management

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

        // MC Server JAR Management

        public ActionResult SelectServerVersion(string jarFile)
        {
            WebConfig.AppSettings["McJarFile"] = jarFile;

            return RedirectToAction("");
        }

        public ActionResult ServerVersions()
        {
            var client = new System.Net.WebClient();
            client.CachePolicy = new RequestCachePolicy(RequestCacheLevel.CacheIfAvailable);

            // Note: versions.json doesn't have Allow-Origin, so it can't be used on the client-side directly!
            var jObj = JObject.Parse(client.DownloadString(
                "https://s3.amazonaws.com/Minecraft.Download/versions/versions.json"));

            var vers = jObj["versions"].Select(t => t["id"].Value<string>()).TakeWhile(v => v != "1.2.4");
            vers = new[] { "Latest Stable", "Latest Snapshot" }.Union(vers);

            return Content("<option>" + string.Join("</option><option>", vers) + "</option>");
        }

        public ActionResult UpdateServer(string ver)
        {
            // http://assets.minecraft.net/ <- This is an XML file
            // http://assets.minecraft.net/V_E_R/minecraft_server.jar
            // Old Stuff, from beta 1.8 pre till 1.5.2, and from 11w47 till 13w12 snapshots

            // https://s3.amazonaws.com/Minecraft.Download/versions/versions.json
            // https://s3.amazonaws.com/Minecraft.Download/versions/V.E.R/minecraft_server.V.E.R.jar
            // Minimum Available Server Version: 1.2.5

            var client = new System.Net.WebClient();
            client.CachePolicy = new RequestCachePolicy(RequestCacheLevel.CacheIfAvailable);

            HttpContext.Application["UpdateProgress"] = "Starting...";

            // Get Latest Version
            if (ver.StartsWith("Latest"))
            {
                var jObj = JObject.Parse(client.DownloadString(
                    "https://s3.amazonaws.com/Minecraft.Download/versions/versions.json"));
                ver = jObj["latest"][ver.Split(' ')[1].ToLower()].Value<string>();
            }

            var jarFile = "minecraft_server." + ver + ".jar";
            var jarUri = "https://s3.amazonaws.com/Minecraft.Download/versions/" + ver + "/" + jarFile;

            var config = WebConfig.OpenWebConfiguration("~");
            var settings = config.AppSettings.Settings;

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

                settings["McJarFile"].Value = jarFile;
                config.Save();
            };

            HttpContext.Application["UpdateProgress"] = "0%";
            System.Threading.Tasks.Task.Run(() => // Workaround to allow Async call
            {
                try
                {
                    client.DownloadFileAsync(new Uri(jarUri), settings["McServerPath"].Value + jarFile);
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

        // MC World Management

        public ActionResult ManageWorlds()
        {
            var backups = new DirectoryInfo(WebConfig.AppSettings["McServerPath"])
                .EnumerateFiles("*.backup.zip")
                .OrderByDescending(fi => fi.LastWriteTimeUtc)
                .Select(fi => Tuple.Create(
                    fi.Name.Replace(".backup.zip", ""),
                    fi.LastWriteTimeUtc))
                .ToList();

            return View(backups);
        }

        public ActionResult BackupWorld(string name)
        {
            var serverPath = WebConfig.AppSettings["McServerPath"];

            ZipFile.CreateFromDirectory(
                Path.Combine(serverPath, "World"),
                Path.Combine(serverPath, name + ".backup.zip"),
                CompressionLevel.Fastest, includeBaseDirectory: false);

            return RedirectToAction("ManageWorlds");
        }

        public ActionResult RestoreWorld(string name)
        {
            var serverPath = WebConfig.AppSettings["McServerPath"];

            Directory.Delete(Path.Combine(serverPath, "World"), true);

            ZipFile.ExtractToDirectory(
                Path.Combine(serverPath, name + ".backup.zip"),
                Path.Combine(serverPath, "World"));

            return RedirectToAction("ManageWorlds");
        }

        public ActionResult DeleteWorldBackup(string name)
        {
            System.IO.File.Delete(
                Path.Combine(WebConfig.AppSettings["McServerPath"], name + ".backup.zip"));

            return RedirectToAction("ManageWorlds");
        }
    }
}
