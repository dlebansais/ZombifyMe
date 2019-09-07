namespace ZombifyMe
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Threading;

    public class Zombification
    {
        #region Init
        public Zombification(string clientName)
        {
            ClientName = clientName;
        }
        #endregion

        #region Properties
        public static bool IsRestart
        {
            get
            {
                try
                {
                    IDictionary EnvironmentVariables = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Process);
                    return EnvironmentVariables.Contains(Shared.RestartEnvironmentVariable);
                }
                catch
                {
                    return false;
                }
            }
        }

        public string ClientName { get; }
        public TimeSpan Delay { get; set; } = TimeSpan.Zero;
        public bool SameCommandLine { get; set; } = true;
        public bool RestartedBallonMessage { get; set; } = false;
        #endregion

        #region Client Interface
        public bool ZombifyMe()
        {
            Assembly EntryAssembly = Assembly.GetEntryAssembly();
            string ClientExePath = EntryAssembly.Location;

            if (!LoadMonitor(out string MonitorProcessFileName))
                return false;

            int ProcessId = Process.GetCurrentProcess().Id;
            long DelayTicks = Delay.Ticks;

            CancelEvent = new EventWaitHandle(false, EventResetMode.ManualReset, Shared.GetCancelEventName(ClientName));

            Process MonitorProcess = new Process();
            MonitorProcess.StartInfo.FileName = MonitorProcessFileName;
            MonitorProcess.StartInfo.Arguments = $"{ProcessId} \"{ClientExePath}\" \"\" \"{ClientName}\" {DelayTicks}";
            MonitorProcess.StartInfo.UseShellExecute = false;
            MonitorProcess.StartInfo.CreateNoWindow = true;

            bool Result;

            try
            {
                Result = MonitorProcess.Start();
            }
            catch
            {
                Result = false;
            }

            return true;
        }

        public void Cancel()
        {
            if (CancelEvent != null)
            {
                using (EventWaitHandle Handle = CancelEvent)
                {
                    CancelEvent = null;
                    Handle.Set();
                }
            }
        }
        #endregion

        #region Implementation
        private bool LoadMonitor(out string fileName)
        {
            fileName = null;

            Assembly CurrentAssembly = Assembly.GetExecutingAssembly();
            string[] ResourceNames = CurrentAssembly.GetManifestResourceNames();
            foreach (string ResourceName in ResourceNames)
                if (ResourceName.EndsWith(".exe"))
                {
                    using (Stream ResourceStream = CurrentAssembly.GetManifestResourceStream(ResourceName))
                    {
                        return LoadMonitor(ResourceStream, out fileName);
                    }
                }

            return false;
        }

        private bool LoadMonitor(Stream resourceStream, out string fileName)
        {
            try
            {
                fileName = Path.GetTempPath() + Guid.NewGuid().ToString() + ".exe";

                using (FileStream FileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    resourceStream.CopyTo(FileStream);
                }

                NativeMethods.MoveFileEx(fileName, null, NativeMethods.MoveFileFlags.DelayUntilReboot);

                return true;
            }
            catch
            {
                fileName = null;
                return false;
            }
        }

        private EventWaitHandle CancelEvent;
        #endregion
    }
}
