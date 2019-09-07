namespace ZombifyMe
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Threading;

    /// <summary>
    /// This class provides an API to restart a process after it has crashed.
    /// </summary>
    public class Zombification
    {
        #region Init
        /// <summary>
        /// Initializes a new instance of the <see cref="Zombification"/> class.
        /// </summary>
        /// <param name="clientName">Name of the client.</param>
        public Zombification(string clientName)
        {
            ClientName = clientName;
        }
        #endregion

        #region Properties
        /// <summary>
        /// True if the current process is a restarted one.
        /// </summary>
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

        /// <summary>
        /// Gets the name of the client.
        /// </summary>
        public string ClientName { get; }

        /// <summary>
        /// Gets and sets the delay between when a process has crashed and when it's restarted.
        /// </summary>
        public TimeSpan Delay { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// Message to display when watching begins.
        /// </summary>
        public string WatchingMessage { get; set; } = null;

        /// <summary>
        /// Message to display after a process is restarted.
        /// </summary>
        public string RestartMessage { get; set; } = "ZombifyMe Alert:\nA protected process has been restarted";

        /// <summary>
        /// Flags for the restarted process.
        /// </summary>
        public Flags Flags { get; set; } = Flags.ForwardArguments;

        /// <summary>
        /// The last error encountered by <see cref="ZombifyMe"/>.
        /// </summary>
        public Errors LastError { get; private set; } = Errors.Success;
        #endregion

        #region Client Interface
        /// <summary>
        /// Installs the monitor.
        /// </summary>
        /// <returns>True if successful; False otherwise and <see cref="LastError"/> contains the error.</returns>
        public bool ZombifyMe()
        {
            LastError = Errors.Success;

            if (!LoadMonitor(out string MonitorProcessFileName))
            {
                LastError = Errors.UnableToLoadSource;
                return false;
            }

            int ProcessId = Process.GetCurrentProcess().Id;

            Assembly EntryAssembly = Assembly.GetEntryAssembly();
            string ClientExePath = EntryAssembly.Location;

            string ArgsText = string.Empty;
            if (Flags.HasFlag(Flags.ForwardArguments))
            {
                // Accumulate arguments in a single string.
                string[] Args = Environment.GetCommandLineArgs();
                for (int i = 1; i < Args.Length; i++)
                {
                    string Arg = Args[i];
                    if (ArgsText.Length > 0)
                        ArgsText += " ";

                    ArgsText += Arg;
                }
            }

            long DelayTicks = Delay.Ticks;

            CancelEvent = new EventWaitHandle(false, EventResetMode.ManualReset, Shared.GetCancelEventName(ClientName));

            // Start the monitoring process.
            Process MonitorProcess = new Process();
            MonitorProcess.StartInfo.FileName = MonitorProcessFileName;
            MonitorProcess.StartInfo.Arguments = $"{ProcessId} \"{ClientExePath}\" \"{ArgsText}\" \"{ClientName}\" {DelayTicks} \"{WatchingMessage}\" \"{RestartMessage}\" {(int)Flags}";
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

            if (!Result)
                LastError = Errors.MonitorNotStarted;

            return Result;
        }

        /// <summary>
        /// Cancels the monitoring of a process.
        /// </summary>
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
        /// <summary>
        /// Loads the first executable in resources and write it down to a temporary file.
        /// </summary>
        /// <param name="fileName">The executable file name upon return.</param>
        /// <returns>True if the file could be loaded and copied.</returns>
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

        /// <summary>
        /// Writes down a stream to a temporary file.
        /// </summary>
        /// <param name="resourceStream">The stream to copy.</param>
        /// <param name="fileName">The executable file name upon return.</param>
        /// <returns>True if the file could be loaded and copied.</returns>
        private bool LoadMonitor(Stream resourceStream, out string fileName)
        {
            try
            {
                fileName = Path.GetTempPath() + Guid.NewGuid().ToString() + ".exe";

                using (FileStream FileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    resourceStream.CopyTo(FileStream);
                }

                // Ensure the file will be deleted when the system has rebooted.
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
