using TauCode.Cli;
using TauCode.Cli.HostRunners;

namespace TauCode.Working.TestDemo.Client
{
    class Program
    {
        static int Main(string[] args)
        {
            var runner = new DemoHostRunner(
                "idle",
                new ICliHost[]
                {
                    new WorkerHost(),
                });

            var res = runner.Run(args);
            return res;
        }
    }
}
