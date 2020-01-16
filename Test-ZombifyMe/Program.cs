using System;
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
            bool IsCoverageCancel = args.Length > 0 && args[0] == "coverageCancel";
            bool IsCoverageNoForward = args.Length > 0 && args[0] == "coverageNoForward";
            bool IsCoverageBadFolder = args.Length > 0 && args[0] == "coverageBadFolder";
            bool IsCoverageNotSymetric = args.Length > 0 && args[0] == "coverageNotSymetric";
            bool IsManual = args.Length > 0 && args[0] == "manual";

            string Message = $"IsRestart: {IsRestart}, Arguments: {args.Length}";
            foreach (string Arg in args)
                Message += ", " + Arg;

            ShowDialog(IsManual, IsCoverageCancel, Message, MessageBoxButtons.OK);

            Zombification Zombification = new Zombification("test");
            Zombification.Delay = TimeSpan.FromSeconds(5);
            if (IsCoverageNoForward)
                Zombification.Flags = Flags.NoWindow;
            else
                Zombification.Flags = Flags.ForwardArguments | Flags.NoWindow;
            Zombification.IsSymetric = !IsCoverageNotSymetric;
            Zombification.AliveTimeout = TimeSpan.FromSeconds(10);
            if (IsCoverageBadFolder)
                Zombification.MonitorFolder = "*";
            Zombification.ZombifyMe();

            DialogResult ShowResult = ShowDialog(IsManual, IsCoverageCancel, "ZombifyMe() done", MessageBoxButtons.OKCancel);

            Thread.Sleep(5000);
            Zombification.SetAlive();
            Thread.Sleep(5000);
            Zombification.SetAlive();
            Thread.Sleep(5000);
            Zombification.SetAlive();
            Thread.Sleep(5000);
            Zombification.SetAlive();

            if (IsRestart || ShowResult == DialogResult.Cancel)
            {
                Zombification.Cancel();

                ShowDialog(IsManual, IsCoverageCancel, "Cancel() done", MessageBoxButtons.OK);
            }
        }

        private static DialogResult ShowDialog(bool IsManual, bool isCoverageCancel, string text, MessageBoxButtons buttons)
        {
            if (IsManual)
                return MessageBox.Show(text, "", buttons);
            else if (isCoverageCancel)
                return DialogResult.Cancel;
            else
                return DialogResult.OK;
        }
    }
}
