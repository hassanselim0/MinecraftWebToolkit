using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace MinecraftWebToolkit.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult Login()
        {
            if (Membership.GetUser() != null)
                return RedirectToAction("Profile");

            return View();
        }

        [HttpPost]
        public ActionResult Login(string username, string password, string returnUrl)
        {
            if (Membership.ValidateUser(username, password))
            {
                FormsAuthentication.SetAuthCookie(username, true);

                if (!String.IsNullOrEmpty(returnUrl)) return Redirect(returnUrl);

                return RedirectToAction("Profile");
            }

            ViewData["error"] = "Incorrect Username and Password combination !";
            ViewData["returnUrl"] = returnUrl;
            return View();
        }

        public ActionResult Logout()
        {
            var user = Membership.GetUser();
            MvcApplication.McServer.UserIPs.Remove(user.UserName);
            MvcApplication.McServer.UserLastPing.Remove(user.UserName);

            FormsAuthentication.SignOut();

            return RedirectToAction("", "");
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(string username, string password, string email)
        {
            try
            {
                MembershipCreateStatus status;
                var user = Membership.CreateUser(username, password, email, null, null, false, out status);
                if (status != MembershipCreateStatus.Success) throw new Exception("Failed: " + status);
                Roles.AddUserToRole(username, "Player");

                FormsAuthentication.SetAuthCookie(username, false);
                return RedirectToAction("Profile");
            }
            catch (Exception e)
            {
                ViewData["error"] = e.Message;
                if (e.InnerException != null)
                    ViewData["error"] += "<br />\r\n" + e.InnerException.Message;
                return View();
            }
        }

        [Authorize, HttpPost]
        public ActionResult ChangePassword(string username, string oldPass, string newPass)
        {
            var usr = username;

            if (string.IsNullOrEmpty(username))
                username = Membership.GetUser().UserName;
            else if (!Roles.IsUserInRole("Admin")) // Prevent non-admins from chaning other users' passwords
            {
                FormsAuthentication.RedirectToLoginPage();
                return Redirect(FormsAuthentication.LoginUrl);
            }

            try
            {
                if (oldPass == "" && Roles.IsUserInRole("Admin"))
                    oldPass = Membership.GetUser(username).ResetPassword();

                var success = Membership.GetUser(username).ChangePassword(oldPass, newPass);
                if (!success) throw new Exception("Incorrect Password!");
            }
            catch (Exception e)
            {
                TempData["error"] = e.Message;
            }

            if (usr == null)
                return RedirectToAction("Profile");

            return RedirectToAction("Profile", new { username = username });
        }

        [Authorize]
        [ActionName("Profile")]
        public ActionResult ShowProfile(string username)
        {
            if (Session["WhitelistUntil"] != null && (DateTime)Session["WhitelistUntil"] < DateTime.Now)
                Session["WhitelistUntil"] = null;

            if (username == null)
            {
                username = Membership.GetUser().UserName;

                MvcApplication.McServer.UserIPs[username] = Request.UserHostAddress;
                MvcApplication.McServer.UserLastPing[username] = DateTime.UtcNow;
            }
            else if (!Roles.IsUserInRole("Moderator")) // Prevent non-moderators from viewing other users
            {
                FormsAuthentication.RedirectToLoginPage();
                return Redirect(FormsAuthentication.LoginUrl);
            }

            return View(Membership.GetUser(username));
        }

        [Authorize]
        public ActionResult Whitelist(string username)
        {
            var usr = username;

            if (username == null)
                username = Membership.GetUser().UserName;
            else if (!Roles.IsUserInRole("Moderator")) // Prevent non-moderators from White-listing other users
            {
                FormsAuthentication.RedirectToLoginPage();
                return Redirect(FormsAuthentication.LoginUrl);
            }

            MvcApplication.McServer.SendCommand("whitelist add " + username);
            if (username == null)
                Session["WhitelistUntil"] = DateTime.Now.AddMinutes(2);

            new System.Threading.Thread(() =>
            {
                System.Threading.Thread.Sleep(TimeSpan.FromMinutes(2));
                MvcApplication.McServer.SendCommand("whitelist remove " + username);
            }).Start();

            if (usr == null)
                return RedirectToAction("Profile");

            return RedirectToAction("Profile", new { username = username });
        }
    }
}
