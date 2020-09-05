using EasyNetQ;
using System.Collections.Generic;
using TauCode.Cli;

namespace TauCode.Working.TestDemo.Client
{
    public class WorkerHost : CliHostBase
    {
        public WorkerHost(IBus bus)
            : base("client", "1.0", true)
        {
            this.Bus = bus;
        }

        public IBus Bus { get; }

        protected override IReadOnlyList<ICliAddIn> CreateAddIns()
        {
            return new ICliAddIn[]
            {
                new WorkerAddIn(),
            };
        }
    }
}
