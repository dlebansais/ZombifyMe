namespace ZombifyMe
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Security;
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
        /// <exception cref="SecurityException">The caller does not have read permission for process environment variables.</exception>
        public static bool IsRestart
        {
            get
            {
                IDictionary EnvironmentVariables = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Process);
                return EnvironmentVariables.Contains(SharedDefinitions.RestartEnvironmentVariable);
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
        public string WatchingMessage { get; set; } = string.Empty;

        /// <summary>
        /// Message to display after a process is restarted.
        /// </summary>
        public string RestartMessage { get; set; } = "ZombifyMe Alert:\nA protected process has been restarted";

        /// <summary>
        /// Flags for the restarted process.
        /// </summary>
        public Flags Flags { get; set; } = Flags.ForwardArguments;

        /// <summary>
        /// True if the main process should also watch on the monitoring process to restart it.
        /// </summary>
        public bool IsSymetric { get; set; } = false;

        /// <summary>
        /// Gets and sets the timeout for the main thread to notify it's alive.
        /// </summary>
        public TimeSpan AliveTimeout { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// Folder where to put the monitoring processes. If empty, the temporary folder is used.
        /// </summary>
        public string MonitorFolder { get; set; } = string.Empty;

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
            Monitoring NewMonitoring = new Monitoring() { ClientName = ClientName, Delay = Delay, WatchingMessage = WatchingMessage, RestartMessage = RestartMessage, Flags = Flags, IsSymetric = IsSymetric, AliveTimeout = AliveTimeout, MonitorFolder = MonitorFolder };
            Debug.Assert(NewMonitoring.CancelEvent == null);

            bool Result = ZombifyMeInternal(NewMonitoring, out Errors Error);
            CancelEvent = NewMonitoring.CancelEvent;
            LastError = Error;

            return Result;
        }

        private static bool ZombifyMeInternal(Monitoring monitoring, out Errors error)
        {
            error = Errors.Success;

            if (!LoadMonitor(monitoring.MonitorFolder, out string MonitorProcessFileName))
            {
                error = Errors.UnableToLoadSource;
                return false;
            }

            int ProcessId = Process.GetCurrentProcess().Id;

            Assembly EntryAssembly = Assembly.GetEntryAssembly();
            string ClientExePath = EntryAssembly.Location;

            string ArgsText = string.Empty;
            if (monitoring.Flags.HasFlag(Flags.ForwardArguments))
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

            long DelayTicks = monitoring.Delay.Ticks;

            monitoring.CancelEvent = new EventWaitHandle(false, EventResetMode.ManualReset, SharedDefinitions.GetCancelEventName(monitoring.ClientName));

            // Use this impossible value as a hack to fail the launch.
            if (monitoring.Delay == TimeSpan.MinValue)
                MonitorProcessFileName = string.Empty;

            // Start the monitoring process.
            // Don't dispose of it, it's passed to another thread.
#pragma warning disable CA2000 // Dispose objects before losing scope
            Process MonitorProcess = new Process();
#pragma warning restore CA2000 // Dispose objects before losing scope
            MonitorProcess.StartInfo.FileName = MonitorProcessFileName;
            MonitorProcess.StartInfo.Arguments = $"{ProcessId} \"{ClientExePath}\" \"{ArgsText}\" \"{monitoring.ClientName}\" {DelayTicks} \"{monitoring.WatchingMessage}\" \"{monitoring.RestartMessage}\" {(int)monitoring.Flags}";
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
            {
                error = Errors.MonitorNotStarted;
                monitoring.MonitorProcess = null;
            }
            else
            {
                monitoring.MonitorProcess = MonitorProcess;

                if (monitoring.IsSymetric && monitoring.AliveTimeout > TimeSpan.Zero)
                    StartSymetricWatch(monitoring);
            }

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

        /// <summary>
        /// Tells the monitoring thread the main thread is alive.
        /// </summary>
        public static void SetAlive()
        {
            AliveWatch.Restart();
        }
        #endregion

        #region Symetric Watch Thread
        /// <summary>
        /// Starts a thread to ensure the monitoring process is restarted if it crashed.
        /// </summary>
        /// <param name="monitoring">Monitoring parameters.</param>
        private static void StartSymetricWatch(Monitoring monitoring)
        {
            Thread NewThread = new Thread(new ParameterizedThreadStart(ExecuteSymetricWatch));
            NewThread.Start(monitoring);
        }

        /// <summary>
        /// Ensures the monitoring process is restarted if it crashed.
        /// </summary>
        /// <param name="parameter">The monitoring process.</param>
        private static void ExecuteSymetricWatch(object parameter)
        {
            // Wait a bit to ensure the new process is started.
            // A proper synchronization would be preferable, but at this point I'm too lazy.
            Thread.Sleep(1000);

            Monitoring Monitoring = (Monitoring)parameter;
            Debug.Assert(Monitoring.MonitorProcess != null);
            Debug.Assert(Monitoring.CancelEvent != null);

#pragma warning disable CS8604 // Possible null reference argument.
            ExecuteSymetricWatch(Monitoring, Monitoring.MonitorProcess, Monitoring.CancelEvent);
#pragma warning restore CS8604 // Possible null reference argument.
        }

        /// <summary>
        /// Ensures the monitoring process is restarted if it crashed.
        /// </summary>
        /// <param name="monitoring">The monitoring information.</param>
        /// <param name="monitorProcess">The monitoring process.</param>
        /// <param name="cancelEvent">The cancelation event.</param>
        private static void ExecuteSymetricWatch(Monitoring monitoring, Process monitorProcess, EventWaitHandle cancelEvent)
        {
            TimeSpan AliveTimeout = monitoring.AliveTimeout;
            Debug.Assert(AliveTimeout > TimeSpan.Zero);

            bool IsAlive = true;
            AliveWatch.Start();

            while (true)
            {
                bool Exit = false;

                if (!IsAlive)
                    Exit = true;

                if (AliveWatch.Elapsed >= AliveTimeout)
                    Exit = true;

                if (Exit)
                    break;

                if (cancelEvent.WaitOne(SharedDefinitions.CheckInterval))
                    break;

                IsAlive = !monitorProcess.HasExited;

                if (!IsAlive)
                {
                    using (Process p = monitorProcess)
                    {
                    }

                    // Wait the same delay as if restarting the original process.
                    Thread.Sleep(monitoring.Delay);

                    ZombifyMeInternal(monitoring, out Errors Error);
                }
            }
        }

        private static Stopwatch AliveWatch = new Stopwatch();
        #endregion

        #region Implementation
        /// <summary>
        /// Loads the first executable in resources and write it down to a temporary file.
        /// </summary>
        /// <param name="monitorFolder">Folder where to write the file.</param>
        /// <param name="fileName">The executable file name upon return.</param>
        /// <returns>True if the file could be loaded and copied.</returns>
        private static bool LoadMonitor(string monitorFolder, out string fileName)
        {
            fileName = string.Empty;

            string ResourceName = string.Empty;

            Assembly CurrentAssembly = Assembly.GetExecutingAssembly();
            string[] ResourceNames = CurrentAssembly.GetManifestResourceNames();
            Debug.Assert(ResourceNames.Length == 1);

            ResourceName = ResourceNames[0];
            Debug.Assert(ResourceName.Length > 0);

            using (Stream ResourceStream = CurrentAssembly.GetManifestResourceStream(ResourceName))
            {
                return LoadMonitor(monitorFolder, ResourceStream, out fileName);
            }
        }

        /// <summary>
        /// Writes down a stream to a temporary file.
        /// </summary>
        /// <param name="monitorFolder">Folder where to write the file.</param>
        /// <param name="resourceStream">The stream to copy.</param>
        /// <param name="fileName">The executable file name upon return.</param>
        /// <returns>True if the file could be loaded and copied.</returns>
        private static bool LoadMonitor(string monitorFolder, Stream resourceStream, out string fileName)
        {
            try
            {
                string DestinationDirectory;

                if (monitorFolder.Length == 0)
                    DestinationDirectory = Path.GetTempPath();
                else
                    DestinationDirectory = monitorFolder;

                Guid NewGuid = Guid.NewGuid();
                string TemporaryFileName = $"0{NewGuid}.exe"; // 0 in front to locate it easily at the top of processes when sorted by names.

                fileName = Path.Combine(DestinationDirectory, TemporaryFileName);

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
                fileName = string.Empty;
                return false;
            }
        }

        private EventWaitHandle? CancelEvent;
        #endregion
    }
}
