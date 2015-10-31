using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;

namespace McServerWebsite
{
    public class MinecraftServer
    {
        public List<ConsoleMessage> ConsoleHistory { get; private set; }
        public bool IsRunning { get; private set; }
        Process ServerProc;

        public Dictionary<string, string> UserIPs { get; private set; }
        public Dictionary<string, DateTime> UserLastPing { get; private set; }

        public MinecraftServer()
        {
            var startInfo = new ProcessStartInfo(ConfigurationManager.AppSettings["JrePath"],
                "-Xmx1024M -Xms512M -jar minecraft_server.jar nogui");
            startInfo.WorkingDirectory = ConfigurationManager.AppSettings["McServerPath"];
            startInfo.RedirectStandardInput = startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false; // Necessary for Standard Stream Redirection
            startInfo.CreateNoWindow = true;

            ServerProc = new Process();
            ServerProc.StartInfo = startInfo;
            ServerProc.EnableRaisingEvents = true;
            ServerProc.OutputDataReceived += new DataReceivedEventHandler(ServerProc_OutputDataReceived);
            ServerProc.Exited += new EventHandler(ServerProc_Exited);

            ConsoleHistory = new List<ConsoleMessage>();
            UserIPs = new Dictionary<string, string>();
            UserLastPing = new Dictionary<string, DateTime>();
        }

        private void ServerProc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null && e.Data.Contains(ConfigurationManager.AppSettings["AzurePingIP"])) return;

            ConsoleHistory.Add(new ConsoleMessage(e.Data));

            if (e.Data != null && !e.Data.StartsWith("<") && e.Data.Contains("logged in"))
            {
                //eg: "[Server thread/INFO]: hassanselim0[/41.178.126.163:54787] logged in with entity id 186 at (-160.21398467711077, 64.0, 158.30000001192093)"
                var msg = ConsoleHistory.Last().Message;
                var colonIdx = msg.IndexOf(':');
                var username = msg.Substring(colonIdx + 2, msg.IndexOf('[', 22) - colonIdx - 2);
                var slashIdx = msg.IndexOf('/', 22);
                var ip = msg.Substring(slashIdx + 1, msg.IndexOf(':', 22) - slashIdx - 1);

                if (!UserIPs.ContainsKey(username) || UserIPs[username] != ip ||
                    DateTime.UtcNow.Subtract(UserLastPing[username]).TotalMinutes > 2)
                    SendCommand("kick " + username + " You have to login at vm.hassanselim.me/mc and don't close the browser!");
            }
        }

        private void ServerProc_Exited(object sender, EventArgs e)
        {
            // The order of these 2 lines is very important, reversing them will cause an exception
            // and you wont be able to read from the stream when you start the Process again !
            ServerProc.CancelOutputRead();
            ServerProc.Close();

            IsRunning = false;
            KeepAlive.Stop();
        }

        public void Start()
        {
            // This is my dirty way of making sure that I don't start the Server Twice :D
            // If the server isn't running then an exception will be thrown when accessing
            // ServerProc.StartTime and the method wont return ;-)
            try { ServerProc.StartTime.ToString(); return; }
            catch { }

            ServerProc.Start();
            ServerProc.BeginOutputReadLine();

            IsRunning = true;
            KeepAlive.Start();
        }

        public void Stop()
        {
            SendCommand("stop");
        }

        public void SendCommand(string command)
        {
            try { ServerProc.StandardInput.WriteLine(command); }
            catch { }
        }

        public void Kill()
        {
            foreach (var p in Process.GetProcessesByName("java"))
                try { p.Kill(); }
                catch { }

            IsRunning = false;
            KeepAlive.Stop();
        }
    }

    public struct ConsoleMessage
    {
        public DateTime Timestamp { get; private set; }
        public String Message { get; private set; }

        public ConsoleMessage(string line)
            : this()
        {
            if (line == null)
            {
                Timestamp = DateTime.Now;
                Message = "Server closed !";
                return;
            }

            try
            {
                //eg: "[14:44:27] [Server thread/INFO]: Loading properties"
                var idx1 = line.IndexOf('[');
                var idx2 = line.IndexOf(']');
                Timestamp = DateTime.Parse(line.Substring(idx1 + 1, idx2 - idx1 - 1));
                Timestamp.AddMilliseconds(DateTime.UtcNow.Millisecond);
                Message = line.Substring(idx2 + 2);
            }
            catch
            {
                Timestamp = DateTime.Now;
                Message = line;
            }
        }

        public override string ToString()
        {
            return Timestamp.ToLongTimeString() + " - " + Message;
        }
    }
}