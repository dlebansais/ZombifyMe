﻿namespace ZombifyMe
{
    using System;

    /// <summary>
    /// Provides common definitions for the ZombifyMe library and the monitoring program.
    /// </summary>
    public static class SharedDefinitions
    {
        #region Constants
        /// <summary>
        /// Unique name to use for the cancellation event.
        /// </summary>
        private const string CancelEventName = "{02702794-5BFC-4C42-8714-AEACCB337019}";

        /// <summary>
        /// Name of the environment variable to use to indicate a process is a restarted one.
        /// </summary>
        internal const string RestartEnvironmentVariable = "{8DCA9D40-5228-4ECE-85EA-FCEE0DC75E76}";

        /// <summary>
        /// The period for checking if a process has disappeared.
        /// </summary>
        internal static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(1);
        #endregion

        #region Tools
        /// <summary>
        /// Gets the unique name of the cancellation event for a given client.
        /// </summary>
        /// <param name="clientName">The client name.</param>
        /// <returns>The complete event name.</returns>
        internal static string GetCancelEventName(string clientName)
        {
            return $"{CancelEventName}-{clientName}";
        }
        #endregion
    }
}
