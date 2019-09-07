﻿namespace ZombifyMe
{
    /// <summary>
    /// Errors reported by the ZombifyMe API.
    /// </summary>
    public enum Errors
    {
        /// <summary>
        /// The call was successful.
        /// </summary>
        Success,

        /// <summary>
        /// An internal resource could not be loaded.
        /// </summary>
        UnableToLoadSource,

        /// <summary>
        /// The monitoring process could not be started.
        /// </summary>
        MonitorNotStarted,
    }
}
