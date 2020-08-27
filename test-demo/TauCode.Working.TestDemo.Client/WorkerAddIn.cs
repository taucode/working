using System.Collections.Generic;
using TauCode.Cli;
using TauCode.Working.TestDemo.Client.Executors;

namespace TauCode.Working.TestDemo.Client
{
    /// <summary>
    /// Nameless add-in
    /// </summary>
    public class WorkerAddIn : CliAddInBase
    {
        public WorkerAddIn()
            : base(null, null, false)
        {
        }

        protected override void OnNodeCreated()
        {
        }

        protected override IReadOnlyList<ICliExecutor> CreateExecutors()
        {
            return new ICliExecutor[]
            {
                new StartWorkerExecutor(),
                new StopWorkerExecutor(),
                new GetWorkerNameExecutor(),
                new GetWorkerStateExecutor(),
            };
        }
    }
}
