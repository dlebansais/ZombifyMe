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
            bool IsManual = args.Length > 0 && args[0] == "manual";

            string Message = $"IsRestart: {IsRestart}, Arguments: {args.Length}";
            foreach (string Arg in args)
                Message += ", " + Arg;

            ShowDialog(IsManual, IsCoverageCancel, Message, MessageBoxButtons.OK);

            if (IsMonitor)
            {
                bool IsMonitorcancel = args.Length > 1 && args[1] == "cancel";

                Console.WriteLine($"set TESTZOMBIFY_PROCESSID={Process.GetCurrentProcess().Id}\r\n");
                using EventWaitHandle CancelEvent = new EventWaitHandle(false, EventResetMode.ManualReset, SharedDefinitions.GetCancelEventName("Coverage"));
                Thread.Sleep(5000);
                if (IsMonitorcancel)
                {
                    CancelEvent.Set();
                    Thread.Sleep(1000);
                }
                return;
            }

            Zombification Zombification = new Zombification("test");

            if (IsCoverageFailLaunch)
                Zombification.Delay = TimeSpan.MinValue;
            else if (IsCoverageFailSymetric)
                Zombification.Delay = TimeSpan.MaxValue;
            else
                Zombification.Delay = TimeSpan.FromSeconds(5);

            if (IsCoverageNoForward)
                Zombification.Flags = Flags.NoWindow;
            else
                Zombification.Flags = Flags.ForwardArguments | Flags.NoWindow;

            Zombification.IsSymetric = !IsCoverageNotSymetric;
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
