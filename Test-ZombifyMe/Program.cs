using System;
using System.Threading;
using System.Windows.Forms;
using ZombifyMe;

namespace TestZombifyMe
{
    public static class Program
    {
        public static bool IsRestart { get { return Zombification.IsRestart; } }

        private static void Main(string[] args)
        {
            bool IsCoverageOk = args.Length > 0 && args[0] == "coverageOk";
            bool IsCoverageCancel = args.Length > 0 && args[0] == "coverageCancel";

            string Message = $"IsRestart: {IsRestart}, Arguments: {args.Length}";
            foreach (string Arg in args)
                Message += ", " + Arg;

            ShowDialog(IsCoverageOk, IsCoverageCancel, Message, MessageBoxButtons.OK);

            Zombification Zombification = new Zombification("test");
            Zombification.Delay = TimeSpan.FromSeconds(5);
            Zombification.Flags = Flags.ForwardArguments | Flags.NoWindow;
            Zombification.IsSymetric = true;
            Zombification.AliveTimeout = TimeSpan.FromSeconds(10);
            Zombification.ZombifyMe();

            DialogResult ShowResult = ShowDialog(IsCoverageOk, IsCoverageCancel, "ZombifyMe() done", MessageBoxButtons.OKCancel);

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

                ShowDialog(IsCoverageOk, IsCoverageCancel, "Cancel() done", MessageBoxButtons.OK);
            }
        }

        private static DialogResult ShowDialog(bool isCoverageOk, bool isCoverageCancel, string text, MessageBoxButtons buttons)
        {
            if (isCoverageOk)
                return DialogResult.OK;
            else if (isCoverageCancel)
                return DialogResult.Cancel;
            else
                return MessageBox.Show(text, "", buttons);
        }
    }
}
