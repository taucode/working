using EasyNetQ;
using TauCode.Cli;
using TauCode.Cli.HostRunners;

namespace TauCode.Working.TestDemo.Cui.Client
{
    public class WorkerHostRunner : DemoHostRunner
    {
        public WorkerHostRunner(IBus bus)
            : base(
                "idle",
                new ICliHost[]
                {
                    new WorkerHost(bus),
                })
        {
        }

        protected override void InitHosts()
        {
            base.InitHosts();

            this.SetHost("client");
        }
    }
}
