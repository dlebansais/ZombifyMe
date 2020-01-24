using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using ZombifyMe;

namespace TestZombifyMe
{
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
            bool IsCoverageNotSymetric = args.Length > 0 && args[0] == "coverageNotSymetric";
            bool IsCoverageFailSymetric = args.Length > 0 && args[0] == "coverageFailSymetric";
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

                Console.WriteLine($"set TESTZOMBIFY_PROCESSID={Process.GetCurrentProcess().Id}\r\n");
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

            Zombification Zombification = new Zombification("test");

            if (IsCoverageFailLaunch)
                Zombification.Delay = TimeSpan.MinValue;
            else
                Zombification.Delay = TimeSpan.FromSeconds(5);

            if (IsCoverageNoForward)
                Zombification.Flags = Flags.NoWindow;
            else if (IsCoverageFailSymetric)
                Zombification.Flags = (Flags)(-1);
            else
                Zombification.Flags = Flags.ForwardArguments | Flags.NoWindow;

            Zombification.IsSymetric = !IsCoverageNotSymetric;

            if (IsCoverageNoAliveTimeout)
                Zombification.AliveTimeout = TimeSpan.Zero;
            else
                Zombification.AliveTimeout = TimeSpan.FromSeconds(10);

            if (IsCoverageBadFolder)
                Zombification.MonitorFolder = "*";

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
                return MessageBox.Show(text, "", buttons);
            else if (isCoverageCancel)
                return DialogResult.Cancel;
            else
                return DialogResult.OK;
        }
    }
}
