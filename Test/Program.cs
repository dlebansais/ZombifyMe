using System.Threading;
using ZombifyMe;

namespace Test
{
    public class Program
    {
        public static bool IsRestart { get { return Zombification.IsRestart; } }

        static void Main(string[] args)
        {
            Zombification Zombification = null;

            Zombification = new Zombification("test");
            Zombification.SameCommandLine = true;
            Zombification.RestartedBallonMessage = true;
            Zombification.ZombifyMe();

            Thread.Sleep(10000);

            if (Zombification != null && IsRestart)
                Zombification.Cancel();
        }
    }
}
