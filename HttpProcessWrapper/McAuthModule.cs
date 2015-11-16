using Nancy;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

//TODO: Move this to another assembly that gets discovered and loaded automatically
namespace HttpProcessWrapper
{
    public class McAuthModule : NancyModule
    {
        const string procName = "McServer";

        private static Dictionary<string, string> userIPs = new Dictionary<string, string>();
        private static Dictionary<string, DateTime> userDates = new Dictionary<string, DateTime>();

        public McAuthModule()
        {
            // This get's a higher priority than the one in ProcModule
            Get["/StartProc/" + procName] = p =>
            {
                var procs = ProcModule.Procs;

                if (procs.ContainsKey(procName) && procs[procName].IsRunning)
                    throw new Exception("Minecraft Server Already Running");

                procs[procName] = new McWrapper(Request.Query.cmd, Request.Query.args, Request.Query.dir);

                return 200;
            };

            Get["/McAuth/Renew/{username}"] = p =>
            {
                userIPs[p.username] = Request.Query.ip;
                userDates[p.username] = DateTime.UtcNow;

                return 200;
            };

            Get["/McAuth/Revoke/{username}"] = p =>
            {
                userIPs.Remove(p.username);
                userDates.Remove(p.username);

                return 200;
            };
        }

        public static bool IsAuthorized(string username, string ip)
        {
            return userIPs.ContainsKey(username) && userIPs[username] == ip
                && DateTime.UtcNow.Subtract(userDates[username]).TotalMinutes < 2;
        }
    }

    public class McWrapper : ProcWrapper
    {
        private Dictionary<string, string> userIPs = new Dictionary<string, string>();
        private Dictionary<string, DateTime> userDates = new Dictionary<string, DateTime>();

        public McWrapper(string cmd, string args, string dir) :
            base(cmd, args, dir) { }

        protected override void proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            base.proc_OutputDataReceived(sender, e);

            // What I'm trying to parse looks like this:
            // [hh:mm:ss] [Server thread/INFO]: username[/ip:port] logged in with entity id ### at (X, Y, Z)
            // You can interactively try the regex here: http://regexr.com/3c7gl
            var pattern = @"\[Server thread\/INFO\]: ([\w.]+)\[\/([\w.]+):\d+\] logged in";
            var match = System.Text.RegularExpressions.Regex.Match(e.Data, pattern);

            if (match.Success)
            {
                var username = match.Groups[1].Value;
                var ip = match.Groups[2].Value;

                if (!McAuthModule.IsAuthorized(username, ip))
                    WriteLine("kick " + username +
                        " You have to login at the website and don't close your browser!");
            }
        }
    }
}
