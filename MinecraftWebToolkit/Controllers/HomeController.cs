﻿using System;
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
            userPing();

            if (HttpContext.Application["UpdateProgress"] != null)
                return Content("Updating");
            else if (McServer.Inst.IsRunning)
                return Content("Online");
            else
                return Content("Offline");
        }

        private void userPing()
        {
            var username = User.Identity.Name;

            if (string.IsNullOrEmpty(username)) return;

            var userIPs = McServer.Inst.UserIPs;
            var userLastPing = McServer.Inst.UserLastPing;

            if (userLastPing.ContainsKey(username) &&
                DateTime.UtcNow.Subtract(userLastPing[username]).TotalMinutes < 1)
                return;

            var user = System.Web.Security.Membership.GetUser(username);
            if (user != null && user.IsApproved)
            {
                userIPs[user.UserName] = Request.UserHostAddress;
                userLastPing[user.UserName] = DateTime.UtcNow;
            }
        }
    }
}
