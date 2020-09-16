using System;
using System.Threading.Tasks;

namespace TauCode.Working.ZetaOld.Workers
{
    public sealed class ZetaFuncTimeoutWorker : ZetaTimeoutWorkerBase
    {
        private readonly Func<Task> _taskCreator;

        public ZetaFuncTimeoutWorker(TimeSpan initialTimeout, Func<Task> taskCreator)
            : base(initialTimeout)
        {
            _taskCreator = taskCreator ?? throw new ArgumentNullException(nameof(taskCreator));
        }

        public ZetaFuncTimeoutWorker(int initialMillisecondsTimeout, Func<Task> taskCreator)
            : base(initialMillisecondsTimeout)
        {
            _taskCreator = taskCreator ?? throw new ArgumentNullException(nameof(taskCreator));
        }

        protected override Task DoRealWorkAsync()
        {
            var task = _taskCreator();
            return task;
        }
    }
}
