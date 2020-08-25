using System.Collections.Generic;
using TauCode.Cli;

namespace TauCode.Working.TestDemo.Client
{
    public class WorkerHost : CliHostBase
    {
        public WorkerHost()
            : base("git", "git-1.0", true)
        {
        }

        protected override IReadOnlyList<ICliAddIn> CreateAddIns()
        {
            return new ICliAddIn[]
            {
                new WorkerAddIn(),
            };
        }
    }
}
