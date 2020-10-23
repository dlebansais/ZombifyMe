namespace ZombifyMe
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    /// <summary>
    /// Contains information about how a process should be monitored.
    /// </summary>
    internal class Monitoring
    {
        /// <summary>
        /// Gets or sets the name of the client.
        /// </summary>
        public string ClientName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the delay between when a process has crashed and when it's restarted.
        /// </summary>
        public TimeSpan Delay { get; set; }

        /// <summary>
        /// Gets or sets the message to display when watching begins.
        /// </summary>
        public string WatchingMessage { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the message to display after a process is restarted.
        /// </summary>
        public string RestartMessage { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the flags for the restarted process.
        /// </summary>
        public Flags Flags { get; set; } = Flags.None;

        /// <summary>
        /// Gets or sets a value indicating whether the main process should also watch on the monitoring process to restart it.
        /// </summary>
        public bool IsSymmetric { get; set; }

        /// <summary>
        /// Gets or sets the timeout for the main thread to notify it's alive.
        /// </summary>
        public TimeSpan AliveTimeout { get; set; }

        /// <summary>
        /// Gets or sets the folder where to put the monitoring processes. If empty, the temporary folder is used.
        /// </summary>
        public string MonitorFolder { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the monitoring process.
        /// </summary>
        public Process? MonitorProcess { get; set; }

        /// <summary>
        /// Gets or sets the event used to cancel monitoring.
        /// </summary>
        public EventWaitHandle? CancelEvent { get; set; }
    }
}
