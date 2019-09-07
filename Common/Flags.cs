namespace ZombifyMe
{
    /// <summary>
    /// Flags that indicate how a process should be restarted.
    /// </summary>
    public enum Flags
    {
        /// <summary>
        /// The 'no flags' value.
        /// </summary>
        None = 0,

        /// <summary>
        /// If set, the process is restarted without a new console window.
        /// </summary>
        NoWindow = 0x01,

        /// <summary>
        /// If set, the same arguments of the original process are reused for the restarted process.
        /// </summary>
        ForwardArguments = 0x02,
    }
}
