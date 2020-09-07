using System.Collections.Generic;
using System.Text;
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
            : base(null, null, true)
        {
        }

        protected override void OnNodeCreated()
        {
        }

        protected override string GetHelpImpl()
        {
            var executors = this.GetExecutors();

            var sb = new StringBuilder();

            foreach (var executor in executors)
            {
                sb.AppendLine(executor.Descriptor.Verb);
            }

            return sb.ToString();
        }

        protected override IReadOnlyList<ICliExecutor> CreateExecutors()
        {
            return new ICliExecutor[]
            {
                new GetWorkerInfoExecutor(),
                new StartWorkerExecutor(),
                new PauseWorkerExecutor(),
                new ResumeWorkerExecutor(),
                new StopWorkerExecutor(),
                new DisposeWorkerExecutor(),

                new TimeoutWorkerExecutor(),

                new QueueWorkerExecutor(),
            };
        }
    }
}
