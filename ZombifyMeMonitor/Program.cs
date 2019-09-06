namespace ZombifyMe
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using TaskbarTools;

    public class Program
    {
        #region Constants
        public static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(10);
        #endregion

        #region Implementation
        public static int Main(string[] args)
        {
            Debug.Assert(args != null && args.Length >= 4);

            if (!int.TryParse(args[0], out int ProcessId))
                return -1;

            string ProcessExePath = args[1];
            string ProcessArguments = args[2];
            string ClientName = args[3];

            if (!OpenCancelEvent(Shared.GetCancelEventName(ClientName), out EventWaitHandle CancelEvent))
                return -2;

            MonitorProcess(ProcessId, ProcessExePath, ProcessArguments, CancelEvent, out bool IsRestarted);

            return IsRestarted ? 1 : 0;
        }

        private static bool OpenCancelEvent(string cancelEventName, out EventWaitHandle cancelEvent)
        {
            cancelEvent = null;

            try
            {
                cancelEvent = EventWaitHandle.OpenExisting(cancelEventName);
                return cancelEvent != null;
            }
            catch
            {
                return false;
            }
        }

        private static void MonitorProcess(int processId, string processExePath, string processArguments, EventWaitHandle cancelEvent, out bool isRestarted)
        {
            while (true)
            {
                if (cancelEvent.WaitOne(CheckInterval))
                {
                    isRestarted = false;
                    break;
                }

                try
                {
                    using (Process MonitoredProcess = Process.GetProcessById(processId))
                    {
                        Thread.Yield();
                    }
                }
                catch
                {
                    isRestarted = RestartProcess(processExePath, processArguments);
                    break;
                }
            }
        }

        private static bool RestartProcess(string processExePath, string processArguments)
        {
            ProcessStartInfo StartInfo = new ProcessStartInfo();
            StartInfo.FileName = processExePath;
            StartInfo.Arguments = processArguments;
            StartInfo.WorkingDirectory = Path.GetDirectoryName(processExePath);
            StartInfo.UseShellExecute = false;
            StartInfo.CreateNoWindow = true;
            StartInfo.EnvironmentVariables[Shared.RestartEnvironmentVariable] = "*";

            bool Result;

            try
            {
                Result = Process.Start(StartInfo) != null;

                TaskbarBalloon.Show($"ZombifyMe Alert:\nA protected process has been restarted");
            }
            catch
            {
                Result = false;
            }

            return Result;
        }
        #endregion
    }
}
