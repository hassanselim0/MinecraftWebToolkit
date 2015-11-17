using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HttpProcessWrapper
{
    public class ProcModule : NancyModule
    {
        public static Dictionary<string, ProcWrapper> Procs { get; private set; }

        static ProcModule()
        {
            Procs = new Dictionary<string, ProcWrapper>();
        }

        public ProcModule()
        {
            Before += ctx =>
            {
                var isHtml = ctx.Request.Headers.Accept.Any(t => t.Item1.Contains("html"));
                ctx.Parameters.sep = isHtml ? "<br />\r\n" : "\r\n";

                return null;
            };

            Get["/GetProcs"] = p =>
                string.Join(p.sep, Procs.Where(proc => proc.Value.IsRunning).Select(proc => proc.Key));

            Get["/IsRunning/{name}"] = p =>
                (Procs.ContainsKey(p.name) && Procs[p.name].IsRunning).ToString();

            Get["/StartProc/{name}"] = p =>
            {
                if (Procs.ContainsKey(p.name) && Procs[p.name].IsRunning)
                    throw new Exception("Process Already Running");

                Procs[p.name] = new ProcWrapper(Request.Query.cmd, Request.Query.args, Request.Query.dir);

                return 200;
            };

            Get["/GetLog/{name}"] = p =>
            {
                if (!Procs.ContainsKey(p.name)) return "";

                return Procs[p.name].GetLog(Request.Query.since, p.sep);
            };

            Get["/GetLastOutput/{name}"] = p =>
            {
                if (!Procs.ContainsKey(p.name)) return "";

                return Procs[p.name].GetLastOutput();
            };

            Get["/WriteLine/{name}"] = p =>
            {
                Procs[p.name].WriteLine(Request.Query.line);

                return 200;
            };

            Get["/ClearLog/{name}"] = p =>
            {
                Procs[p.name].ClearLog();

                return 200;
            };

            Get["/KillProc/{name}"] = p =>
            {
                Procs[p.name].Kill();

                return 200;
            };
        }

        public static void KillAllProcs()
        {
            foreach (var p in Procs.Values)
                if (p.IsRunning)
                    p.Kill();
        }
    }
}
