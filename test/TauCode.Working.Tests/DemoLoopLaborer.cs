using System;
using System.Threading;
using System.Threading.Tasks;

namespace TauCode.Working.Tests
{
    public class DemoLoopLaborer : LoopWorkerBase
    {
        public override bool IsPausingSupported => true;

        protected override async Task<TimeSpan> DoLabor(CancellationToken cancellationToken)
        {
            return await this.LaborAction(this, cancellationToken);
        }

        public Func<LoopWorkerBase, CancellationToken, Task<TimeSpan>> LaborAction { get; set; }
    }
}
