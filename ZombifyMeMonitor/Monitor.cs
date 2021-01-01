namespace ZombifyMeMonitor
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using TaskbarTools;
    using ZombifyMe;

    /// <summary>
    /// The main class for this program.
    /// </summary>
    public static class Monitor
    {
        /// <summary>
        /// Entry points of the program.
        /// </summary>
        /// <param name="args">Arguments from the ZombifyMe library.</param>
        /// <returns>0 if the monitored process exited normally, 1 if it was restarted, or a negative value in case of error.</returns>
        public static int Main(string[] args)
        {
            // Check arguments; They should be valid since only ZombifyMe is starting us.
#pragma warning disable CA1062 // Validate arguments of public methods
            if (args.Length < 8)
#pragma warning restore CA1062 // Validate arguments of public methods
                return -1;

            // Read the ID of the process to monitor.
            if (!int.TryParse(args[0], out int ProcessId))
                return -2;

            string ProcessExePath = args[1];
            string ProcessArguments = args[2];
            string ClientName = args[3];

            try
            {
                // Open the cancel event. This event uses two unique names, one for the ZombifyMe, the other from the client.
                using EventWaitHandle? CancelEvent = EventWaitHandle.OpenExisting(SharedDefinitions.GetCancelEventName(ClientName));

                // Read the delay, in ticks.
                if (!long.TryParse(args[4], out long DelayTicks))
                    return -4;

                TimeSpan Delay = TimeSpan.FromTicks(DelayTicks);
                if (DelayTicks < 0)
                    return -4;

                // Read messages. They can be empty.
                string WatchingMessage = args[5];
                string RestartMessage = args[6];

                // Read the flags, as a set of bits.
                if (!int.TryParse(args[7], out int FlagsValue))
                    return -5;
                if (FlagsValue == -1)
                    return -5;
                Flags Flags = (Flags)FlagsValue;

                // Display the begin message if requested.
                if (WatchingMessage.Length > 0)
                    TaskbarBalloon.Show(WatchingMessage, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(1));

                MonitorProcess(ProcessId, ProcessExePath, ProcessArguments, CancelEvent, Delay, RestartMessage, Flags, out bool IsRestarted);

                return IsRestarted ? 1 : 0;
            }
            catch
            {
                return -3;
            }
        }

        /// <summary>
        /// Main loop of the monitoring program.
        /// </summary>
        /// <param name="processId">Id of the process to monitor.</param>
        /// <param name="processExePath">Path to the process executable file.</param>
        /// <param name="processArguments">Arguments to use when restarting the process, empty if no argument.</param>
        /// <param name="cancelEvent">An event to check; If set, monitoring is canceled.</param>
        /// <param name="delay">A delay between the time the process has disappeared and the time it's restarted.</param>
        /// <param name="restartMessage">The message to display when a process was restarted.</param>
        /// <param name="flags">Process restart flags.</param>
        /// <param name="isRestarted">True upon return if the process crashed and was restarted.</param>
        private static void MonitorProcess(int processId, string processExePath, string processArguments, EventWaitHandle cancelEvent, TimeSpan delay, string restartMessage, Flags flags, out bool isRestarted)
        {
            isRestarted = false;

            while (processId != 0)
            {
                if (cancelEvent.WaitOne(SharedDefinitions.CheckInterval))
                {
                    isRestarted = false;
                    break;
                }

                try
                {
                    using (Process.GetProcessById(processId))
                    {
                        // Not necessary if CheckInterval is not zero, but let's be a good citizen.
                        Thread.Yield();
                    }
                }
                catch
                {
                    isRestarted = RestartProcess(processExePath, processArguments, delay, restartMessage, flags);
                    processId = 0;
                }
            }
        }

        /// <summary>
        /// Restarts a process that has disappeared.
        /// </summary>
        /// <param name="processExePath">Path to the process executable file.</param>
        /// <param name="processArguments">Arguments to use when restarting the process, empty if no argument.</param>
        /// <param name="delay">A delay between the time the process has disappeared and the time it's restarted.</param>
        /// <param name="restartMessage">The message to display when a process was restarted.</param>
        /// <param name="flags">Process restart flags.</param>
        /// <returns>True if restarted; False if an error occurred.</returns>
        private static bool RestartProcess(string processExePath, string processArguments, TimeSpan delay, string restartMessage, Flags flags)
        {
            if (delay.TotalSeconds > 0)
                Thread.Sleep(delay);

            ProcessStartInfo StartInfo = new ProcessStartInfo
            {
                FileName = processExePath,
                Arguments = processArguments,
                WorkingDirectory = Path.GetDirectoryName(processExePath)!,
            };

            if (flags.HasFlag(Flags.NoWindow))
            {
                StartInfo.UseShellExecute = false;
                StartInfo.CreateNoWindow = true;

                // Setting this variable will tell the process it's been restarted. The value doesn't matter.
                StartInfo.EnvironmentVariables[SharedDefinitions.RestartEnvironmentVariable] = "*";
            }

            bool Result;

            try
            {
                Result = Process.Start(StartInfo) != null;
                Result &= restartMessage.Length > 0;

                if (Result)
                    TaskbarBalloon.Show(restartMessage, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(1));
            }
            catch
            {
                Result = false;
            }

            return Result;
        }
    }
}
