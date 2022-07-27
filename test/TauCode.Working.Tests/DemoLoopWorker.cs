using System;
using System.Threading;
using System.Threading.Tasks;

namespace TauCode.Working.Tests
{
    public class DemoLoopWorker : LoopWorkerBase
    {
        public override bool IsPausingSupported => true;

        protected override async Task<TimeSpan> DoWork(CancellationToken cancellationToken)
        {
            return await this.WorkAction(this, cancellationToken);
        }

        public Func<LoopWorkerBase, CancellationToken, Task<TimeSpan>> WorkAction { get; set; }
    }
}
