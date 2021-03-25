using System;
using System.Threading;
using System.Threading.Tasks;
using TauCode.Working.Labor;

namespace TauCode.Working.Tests.Labor
{
    public class DemoLoopLaborer : LoopLaborerBase
    {
        public override bool IsPausingSupported => true;

        protected override async Task<TimeSpan> DoLabor(CancellationToken cancellationToken)
        {
            return await this.LaborAction(this, cancellationToken);
        }

        public Func<LoopLaborerBase, CancellationToken, Task<TimeSpan>> LaborAction { get; set; }
    }
}
