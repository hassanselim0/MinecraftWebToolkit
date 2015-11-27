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
using FileIO = System.IO.File;
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
            ProcHttpClient.KillProc("McServer");

            return RedirectToAction("");
        }

        public ActionResult StartHPW()
        {
            System.Diagnostics.Process.Start(Server.MapPath(@"~\bin\HPW\HttpProcessWrapper.exe"));

            return RedirectToAction("");
        }

        public ActionResult StopHPW()
        {
            ProcHttpClient.DoAction("/Close");

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
            var config = WebConfig.OpenWebConfiguration("~");
            config.AppSettings.Settings["McJarFile"].Value = jarFile;
            config.Save();

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
            var serverPath = WebConfig.AppSettings["McServerPath"];
            var backupsDir = Path.Combine(serverPath, "Backups");

            if (!Directory.Exists(backupsDir))
                Directory.CreateDirectory(backupsDir);

            ViewBag.Backups =
                new DirectoryInfo(backupsDir)
                .EnumerateFiles("*.zip")
                .OrderByDescending(fi => fi.LastWriteTimeUtc)
                .Select(fi =>
                {
                    var s = fi.Name.Split('.');
                    return Tuple.Create(s[0],
                        DateTime.ParseExact(s[1], "yyyy-MM-dd-hh-mm-ss-tt", null));
                })
                .ToList();

            ViewBag.Worlds =
                Directory.EnumerateDirectories(serverPath)
                .Where(d => FileIO.Exists(d + "\\level.dat"))
                .Select(d => Path.GetFileName(d))
                .ToList();

            ViewBag.SelWorld = McProperties.GetValue("level-name");

            return View();
        }

        public ActionResult CreateWorld(string world)
        {
            world = world.Replace('.', '_'); // Can't have dots!

            var worldPath = Path.Combine(WebConfig.AppSettings["McServerPath"], world);

            Directory.CreateDirectory(worldPath);
            FileIO.WriteAllText(Path.Combine(worldPath, "level.dat"), "");
            // Minecraft Server will show an error for the first time then create a new random world

            return RedirectToAction("ManageWorlds");
        }

        public ActionResult SelectWorld(string world)
        {
            McProperties.SetValue("level-name", world);

            return RedirectToAction("ManageWorlds");
        }

        public ActionResult DeleteWorld(string world)
        {
            Directory.Delete(Path.Combine(WebConfig.AppSettings["McServerPath"], world), true);

            return RedirectToAction("ManageWorlds");
        }

        public ActionResult BackupWorld(string world)
        {
            var serverPath = WebConfig.AppSettings["McServerPath"];
            var zipName = world + DateTime.UtcNow.ToString(".yyyy-MM-dd-hh-mm-ss-tt") + ".zip";

            ZipFile.CreateFromDirectory(
                Path.Combine(serverPath, world),
                Path.Combine(serverPath, "Backups", zipName),
                CompressionLevel.Fastest, includeBaseDirectory: false);

            return RedirectToAction("ManageWorlds");
        }

        public ActionResult RestoreWorld(string world, DateTime date)
        {
            var serverPath = WebConfig.AppSettings["McServerPath"];
            var zipName = world + date.ToString(".yyyy-MM-dd-hh-mm-ss-tt") + ".zip";
            var worldPath = Path.Combine(serverPath, world);

            // Temporarily move the existing world folder
            if (Directory.Exists(worldPath))
                Directory.Move(worldPath, worldPath + "-temp");

            ZipFile.ExtractToDirectory(
                Path.Combine(serverPath, "Backups", zipName), worldPath);

            // Delete the old world folder
            if (Directory.Exists(worldPath + "-temp"))
                Directory.Delete(worldPath + "-temp", true);

            return RedirectToAction("ManageWorlds");
        }

        public ActionResult DeleteWorldBackup(string world, DateTime date)
        {
            FileIO.Delete(Path.Combine(WebConfig.AppSettings["McServerPath"], "Backups",
                world + date.ToString(".yyyy-MM-dd-hh-mm-ss-tt") + ".zip"));

            return RedirectToAction("ManageWorlds");
        }
    }
}
