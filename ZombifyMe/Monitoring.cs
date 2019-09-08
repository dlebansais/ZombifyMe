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
        /// Gets the name of the client.
        /// </summary>
        public string ClientName { get; set; }

        /// <summary>
        /// Gets and sets the delay between when a process has crashed and when it's restarted.
        /// </summary>
        public TimeSpan Delay { get; set; }

        /// <summary>
        /// Message to display when watching begins.
        /// </summary>
        public string WatchingMessage { get; set; }

        /// <summary>
        /// Message to display after a process is restarted.
        /// </summary>
        public string RestartMessage { get; set; }

        /// <summary>
        /// Flags for the restarted process.
        /// </summary>
        public Flags Flags { get; set; }

        /// <summary>
        /// True if the main process should also watch on the monitoring process to restart it.
        /// </summary>
        public bool IsSymetric { get; set; }

        /// <summary>
        /// The monitoring process.
        /// </summary>
        public Process MonitorProcess { get; set; }

        /// <summary>
        /// The event used to cancel monitoring.
        /// </summary>
        public EventWaitHandle CancelEvent { get; set; }
    }
}
