namespace TaskbarTools
{
    using System;
    using System.Drawing;
    using System.Threading;
    using System.Windows.Forms;

    /// <summary>
    /// This class provides an API to display notifications to the user.
    /// </summary>
    public static class TaskbarBalloon
    {
        #region Client Interface
        /// <summary>
        /// Display a notification in a taskbar balloon.
        /// </summary>
        /// <param name="text">The text to show.</param>
        /// <param name="delayShow">The delay showing the balloon.</param>
        /// <param name="delayWait">The delay waiting synchronously to ensure the balloon is entirely visible upon return.</param>
        public static void Show(string text, TimeSpan delayShow, TimeSpan delayWait)
        {
            try
            {
                using (NotifyIcon notification = new NotifyIcon() { Visible = true, Icon = SystemIcons.Shield, Text = text, BalloonTipText = text })
                {
                    notification.ShowBalloonTip((int)delayShow.TotalMilliseconds);
                    Thread.Sleep(delayWait);
                }
            }
            catch
            {
            }
        }
        #endregion
    }
}
