using System;
using System.Threading;
using System.Windows.Forms;
using ZombifyMe;

namespace Test
{
    public class Program
    {
        public static bool IsRestart { get { return Zombification.IsRestart; } }

        static void Main(string[] args)
        {
            string Message = $"IsRestart: {IsRestart}, Arguments: {args.Length}";
            foreach (string Arg in args)
                Message += ", " + Arg;

            MessageBox.Show(Message);

            Zombification Zombification = null;

            Zombification = new Zombification("test");
            Zombification.Delay = TimeSpan.FromSeconds(5);
            Zombification.Flags = Flags.ForwardArguments | Flags.NoWindow;
            Zombification.IsSymetric = true;
            Zombification.AliveTimeout = TimeSpan.FromSeconds(10);
            Zombification.ZombifyMe();

            DialogResult ShowResult = MessageBox.Show("ZombifyMe() done", "", MessageBoxButtons.OKCancel);

            Thread.Sleep(5000);
            Zombification.SetAlive();
            Thread.Sleep(5000);
            Zombification.SetAlive();
            Thread.Sleep(5000);
            Zombification.SetAlive();
            Thread.Sleep(5000);
            Zombification.SetAlive();

            if ((Zombification != null && IsRestart) || ShowResult == DialogResult.Cancel)
            {
                Zombification.Cancel();
                MessageBox.Show("Cancel() done");
            }
        }
    }
}
