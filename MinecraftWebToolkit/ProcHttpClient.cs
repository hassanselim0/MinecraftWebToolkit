using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace MinecraftWebToolkit
{
    public static class ProcHttpClient
    {
        private static HttpClient http;

        static ProcHttpClient()
        {
            http = new HttpClient();
            http.BaseAddress = new Uri("http://localhost:25564");

            if (Process.GetProcessesByName("HttpProcessWrapper").Length == 0)
                Process.Start(HttpContext.Current.Server.MapPath(@"~\bin\HPW\HttpProcessWrapper.exe"));
        }

        public static void DoAction(string uri)
        {
            http.GetAsync(uri).Result
                .EnsureSuccessStatusCode();
        }

        public static string GetResult(string uri)
        {
            return http.GetAsync(uri).Result
                .EnsureSuccessStatusCode()
                .Content.ReadAsStringAsync().Result;
        }

        public static bool IsRunning(string name)
        {
            var str = GetResult(string.Format(
                "/IsRunning/{0}", name));

            return bool.Parse(str);
        }

        public static void StartProc(string name, string cmd, string args, string dir)
        {
            DoAction(string.Format(
                "/StartProc/{0}?cmd={1}&args={2}&dir={3}", name, cmd, args, dir));
        }

        public static string GetLog(string name, long since)
        {
            return GetResult(string.Format(
                "/GetLog/{0}?since={1}", name, since));
        }

        public static string GetLastOutput(string name)
        {
            return GetResult(string.Format(
                "/GetLastOutput/{0}", name));
        }

        public static void WriteLine(string name, string line)
        {
            DoAction(string.Format(
                "/WriteLine/{0}?line={1}", name, line));
        }

        public static void ClearLog(string name)
        {
            DoAction(string.Format(
                "/ClearLog/{0}", name));
        }

        public static void Kill(string name)
        {
            DoAction(string.Format(
                "/Kill/{0}", name));
        }
    }

    // TODO: Move this, and other similar "extensions" to another file?
    public static class McAuthHttpClient
    {
        // Trying to reduce the amount of Membership DB queries and Http calls
        // The earlier proved to be an issue on small VMs (like Azure's A1 Basic)
        private static Dictionary<string, DateTime> cachedAuth = new Dictionary<string, DateTime>();

        public static void Renew(string username, string ip)
        {
            if (string.IsNullOrEmpty(username)) return;

            DateTime cachedDate;
            var isCached = cachedAuth.TryGetValue(username, out cachedDate);
            var isApproved = isCached || System.Web.Security.Membership.GetUser(username).IsApproved;
            var isRecent = isCached && DateTime.UtcNow.Subtract(cachedDate).TotalMinutes < 1;

            if (isApproved && !isRecent)
            {
                ProcHttpClient.DoAction(string.Format(
                    "/McAuth/Renew/{0}?ip={1}", username, ip));

                cachedAuth[username] = DateTime.UtcNow;
            }
        }

        public static void Revoke(string username)
        {
            ProcHttpClient.DoAction(string.Format(
                    "/McAuth/Revoke/{0}", username));

            cachedAuth.Remove(username);
        }
    }
}