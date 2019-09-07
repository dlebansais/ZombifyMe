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
            Debug.Assert(args != null && args.Length >= 8);

            if (!int.TryParse(args[0], out int ProcessId))
                return -1;

            string ProcessExePath = args[1];
            string ProcessArguments = args[2];
            string ClientName = args[3];

            if (!OpenCancelEvent(Shared.GetCancelEventName(ClientName), out EventWaitHandle CancelEvent))
                return -2;

            if (!long.TryParse(args[4], out long DelayTicks))
                return -2;

            string WatchingMessage = args[5];
            string RestartMessage = args[6];

            if (!int.TryParse(args[7], out int FlagsValue))
                return -2;

            Flags Flags = (Flags)FlagsValue;

            if (WatchingMessage.Length > 0)
                TaskbarBalloon.Show(WatchingMessage);

            MonitorProcess(ProcessId, ProcessExePath, ProcessArguments, CancelEvent, TimeSpan.FromTicks(DelayTicks), RestartMessage, Flags, out bool IsRestarted);

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

        private static void MonitorProcess(int processId, string processExePath, string processArguments, EventWaitHandle cancelEvent, TimeSpan delay, string restartMessage, Flags flags, out bool isRestarted)
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
                    isRestarted = RestartProcess(processExePath, processArguments, delay, restartMessage, flags);
                    break;
                }
            }
        }

        private static bool RestartProcess(string processExePath, string processArguments, TimeSpan delay, string restartMessage, Flags flags)
        {
            if (delay.TotalSeconds > 0)
                Thread.Sleep(delay);

            ProcessStartInfo StartInfo = new ProcessStartInfo();
            StartInfo.FileName = processExePath;
            StartInfo.Arguments = processArguments;
            StartInfo.WorkingDirectory = Path.GetDirectoryName(processExePath);

            if (flags.HasFlag(Flags.NoWindow))
            {
                StartInfo.UseShellExecute = false;
                StartInfo.CreateNoWindow = true;
                StartInfo.EnvironmentVariables[Shared.RestartEnvironmentVariable] = "*";
            }

            bool Result;

            try
            {
                Result = Process.Start(StartInfo) != null;

                if (Result && restartMessage.Length > 0)
                    TaskbarBalloon.Show(restartMessage);
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
