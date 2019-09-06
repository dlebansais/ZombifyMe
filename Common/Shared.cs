namespace ZombifyMe
{
    public static class Shared
    {
        #region Constants
        private static readonly string CancelEventName = "{02702794-5BFC-4C42-8714-AEACCB337019}";
        internal static readonly string RestartEnvironmentVariable = "{8DCA9D40-5228-4ECE-85EA-FCEE0DC75E76}";
        #endregion

        #region Constants
        internal static string GetCancelEventName(string clientName)
        {
            return $"{CancelEventName}-{clientName}";
        }
        #endregion
    }
}
