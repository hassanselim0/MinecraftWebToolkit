using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace HttpProcessWrapper
{
    public class ProcWrapper
    {
        static Func<Process, bool> IsAssociated;

        public bool IsRunning { get { return IsAssociated(proc) && !proc.HasExited; } }

        protected Process proc;

        private const int maxLogSize = 1024;
        protected Queue<ConsoleOutput> outputLog;

        static ProcWrapper()
        {
            IsAssociated = (Func<Process, bool>)typeof(Process)
                .GetProperty("Associated", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetMethod.CreateDelegate(typeof(Func<Process, bool>));
        }

        public ProcWrapper(string cmd, string args, string dir)
        {
            var startInfo = new ProcessStartInfo(cmd, args);
            startInfo.WorkingDirectory = dir;
            startInfo.RedirectStandardInput = startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false; // Necessary for Standard Stream Redirection
            startInfo.CreateNoWindow = true;

            proc = new Process();
            proc.StartInfo = startInfo;
            proc.EnableRaisingEvents = true;
            proc.OutputDataReceived += new DataReceivedEventHandler(proc_OutputDataReceived);
            proc.Exited += new EventHandler(proc_Exited);

            outputLog = new Queue<ConsoleOutput>(maxLogSize);

            proc.Start();
            proc.BeginOutputReadLine();
        }

        protected virtual void proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null) return;

            if (outputLog.Count == maxLogSize)
                outputLog.Dequeue();

            outputLog.Enqueue(new ConsoleOutput(e.Data));
        }

        protected virtual void proc_Exited(object sender, EventArgs e)
        {
            // The order of these 2 lines is very important, reversing them will cause an exception
            // and you wont be able to read from the stream when you start the Process again !
            proc.CancelOutputRead();
            proc.Close();

            if (outputLog.Count == maxLogSize)
                outputLog.Dequeue();

            outputLog.Enqueue(new ConsoleOutput("Process Ended!"));
        }

        public string GetLog(long since, string sep)
        {
            // I'm trying to optimise here ... pre-emtively ... sorry
            var sb = new StringBuilder(outputLog.Count * 32);

            foreach (var o in outputLog.SkipWhile(o => o.Timestamp <= since))
                sb.AppendFormat("{0}|{1}{2}", o.Timestamp, o.Data, sep);

            return sb.ToString();
        }

        public string GetLastOutput()
        {
            if (outputLog.Count == 0) return null;

            var o = outputLog.Last();

            return string.Format("{0}|{1}", o.Timestamp, o.Data);
        }

        public virtual void WriteLine(string line)
        {
            proc.StandardInput.WriteLine(line);
        }

        public void ClearLog()
        {
            outputLog.Clear();
        }

        public void Kill()
        {
            proc.Kill();
        }
    }

    public struct ConsoleOutput
    {
        public long Timestamp { get; private set; }

        public string Data { get; private set; }

        public ConsoleOutput(string data)
            : this()
        {
            Timestamp = DateTime.UtcNow.Ticks;

            Data = data;
        }
    }
}
