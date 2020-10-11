namespace TestZombifyMe
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Windows.Forms;
    using ZombifyMe;

    public static class Program
    {
        public static bool IsRestart
        {
            get
            {
                try
                {
                    return Zombification.IsRestart;
                }
                catch
                {
                    return false;
                }
            }
        }

        private static void Main(string[] args)
        {
            bool IsMonitor = args.Length > 0 && args[0] == "monitor";
            bool IsCoverageCancel = args.Length > 0 && args[0] == "coverageCancel";
            bool IsCoverageNoForward = args.Length > 0 && args[0] == "coverageNoForward";
            bool IsCoverageBadFolder = args.Length > 0 && args[0] == "coverageBadFolder";
            bool IsCoverageNotSymmetric = args.Length > 0 && args[0] == "coverageNotSymmetric";
            bool IsCoverageFailSymmetric = args.Length > 0 && args[0] == "coverageFailSymmetric";
            bool IsCoverageFailLaunch = args.Length > 0 && args[0] == "coverageFailLaunch";
            bool IsCoverageNoKeepAlive = args.Length > 0 && args[0] == "coverageNoKeepAlive";
            bool IsCoverageNoAliveTimeout = args.Length > 0 && args[0] == "coverageNoAliveTimeout";
            bool IsManual = args.Length > 0 && args[0] == "manual";

            string Message = $"IsRestart: {IsRestart}, Arguments: {args.Length}";
            foreach (string Arg in args)
                Message += ", " + Arg;

            ShowDialog(IsManual, IsCoverageCancel, Message, MessageBoxButtons.OK);

            if (IsMonitor)
            {
                bool IsMonitorCancel = args.Length > 1 && args[1] == "cancel";
                bool IsMonitorWait = args.Length > 1 && args[1] == "cancel";

                Console.WriteLine($"set TEST_ZOMBIFY_PROCESS_ID={Process.GetCurrentProcess().Id}\r\n");
                using EventWaitHandle CancelEvent = new EventWaitHandle(false, EventResetMode.ManualReset, SharedDefinitions.GetCancelEventName("Coverage"));
                Thread.Sleep(TimeSpan.FromSeconds(5));
                if (IsMonitorCancel)
                {
                    CancelEvent.Set();
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                }
                if (IsMonitorWait)
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                return;
            }

            Zombification Zombification = new Zombification("test")
            {
                Delay = IsCoverageFailLaunch ? TimeSpan.MinValue : TimeSpan.FromSeconds(5),
                Flags = IsCoverageNoForward ? Flags.NoWindow : (IsCoverageFailSymmetric ? (Flags)(-1) : Flags.ForwardArguments | Flags.NoWindow),
                IsSymmetric = !IsCoverageNotSymmetric,
                AliveTimeout = IsCoverageNoAliveTimeout ? TimeSpan.Zero : TimeSpan.FromSeconds(10),
                MonitorFolder = IsCoverageBadFolder ? "*" : string.Empty,
            };

            Zombification.Cancel();
            Zombification.ZombifyMe();

            DialogResult ShowResult = ShowDialog(IsManual, IsCoverageCancel, "ZombifyMe() done", MessageBoxButtons.OKCancel);

            if (IsCoverageNoKeepAlive)
                Thread.Sleep(20000);
            else
            {
                Thread.Sleep(5000);
                Zombification.SetAlive();
                Thread.Sleep(5000);
                Zombification.SetAlive();
                Thread.Sleep(5000);
                Zombification.SetAlive();
                Thread.Sleep(5000);
                Zombification.SetAlive();
            }

            if (IsRestart || ShowResult == DialogResult.Cancel)
            {
                Zombification.Cancel();

                ShowDialog(IsManual, IsCoverageCancel, "Cancel() done", MessageBoxButtons.OK);
            }
        }

        private static DialogResult ShowDialog(bool isManual, bool isCoverageCancel, string text, MessageBoxButtons buttons)
        {
            if (isManual)
                return MessageBox.Show(text, string.Empty, buttons);
            else if (isCoverageCancel)
                return DialogResult.Cancel;
            else
                return DialogResult.OK;
        }
    }
}
