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
            string Message = $"IsRestart: {IsRestart}, Arguments: {args.Length}";
            foreach (string Arg in args)
                Message += ", " + Arg;

            bool IsCoverage = args.Length > 1 && args[1] == "coverage";
            bool IsContinue = args.Length > 1 && args[1] == "continue";

            if (IsContinue)
            {
                ShowDialog(false, "Proceeding to tests", MessageBoxButtons.OK);
                return;
            }

            ShowDialog(IsCoverage, Message, MessageBoxButtons.OK);

            Zombification Zombification = new Zombification("test");
            Zombification.Delay = TimeSpan.FromSeconds(5);
            Zombification.Flags = Flags.ForwardArguments | Flags.NoWindow;
            Zombification.IsSymetric = true;
            Zombification.AliveTimeout = TimeSpan.FromSeconds(10);
            Zombification.ZombifyMe();

            DialogResult ShowResult = ShowDialog(IsCoverage, "ZombifyMe() done", MessageBoxButtons.OKCancel);

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

                ShowDialog(IsCoverage, "Cancel() done", MessageBoxButtons.OK);
            }
        }

        private static DialogResult ShowDialog(bool isCoverage, string text, MessageBoxButtons buttons)
        {
            if (isCoverage)
                return DialogResult.Cancel;
            else
                return MessageBox.Show(text, "", buttons);
        }
    }
}
