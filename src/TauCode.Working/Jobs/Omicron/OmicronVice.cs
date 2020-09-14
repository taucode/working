using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TauCode.Labor;

namespace TauCode.Working.Jobs.Omicron
{
    internal class OmicronVice : CycleProlBase
    {
        private readonly Dictionary<string, OmicronEmployee> _employees;

        internal OmicronVice()
        {
            _employees = new Dictionary<string, OmicronEmployee>();
        }

        protected override Task<TimeSpan> DoWork(CancellationToken token)
        {
            return Task.FromResult(TimeSpan.MaxValue); // todo0
        }
    }
}
