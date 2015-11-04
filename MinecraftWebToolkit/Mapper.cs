using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace MinecraftWebToolkit
{
    public class Mapper
    {
        public static string Progress { get; set; }
        static Process proc;

        static Mapper()
        {
            var startInfo = new ProcessStartInfo(ConfigurationManager.AppSettings["McServerPath"] + "Overviewer\\overviewer.exe",
                   "-p 4 --rendermodes=smooth_lighting,smooth_night \"world\" \"Map\"");
            startInfo.WorkingDirectory = ConfigurationManager.AppSettings["McServerPath"];
            startInfo.RedirectStandardInput = startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false; // Necessary for Standard Stream Redirection
            startInfo.CreateNoWindow = true;

            proc = new Process();
            proc.StartInfo = startInfo;
            proc.EnableRaisingEvents = true;
            proc.OutputDataReceived += new DataReceivedEventHandler(proc_OutputDataReceived);
            proc.Exited += new EventHandler(proc_Exited);

            Progress = "";
        }

        private static void proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!e.Data.EndsWith("% complete")) return;

            var str = e.Data.Split(' ')[8];
            if (!str.EndsWith("%")) return;

            Progress = str;
        }

        private static void proc_Exited(object sender, EventArgs e)
        {
            proc.CancelOutputRead();
            proc.Close();

            Progress = "Completed";
            KeepAlive.Stop();
        }

        public static void Start()
        {
            try { var x = proc.StartTime; return; }
            catch { }

            proc.Start();
            proc.BeginOutputReadLine();

            Progress = "0%";
            KeepAlive.Start();
        }

        public static void Kill()
        {
            foreach (var p in Process.GetProcessesByName("overviewer"))
                try { p.Kill(); }
                catch { }

            Progress = "";
            KeepAlive.Stop();
        }
    }
}