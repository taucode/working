using System;
using System.Threading.Tasks;

namespace TauCode.Working
{
    public sealed class FuncTimeoutWorker : TimeoutWorkerBase
    {
        private readonly Func<Task> _taskCreator;

        public FuncTimeoutWorker(TimeSpan initialTimeout, Func<Task> taskCreator)
            : base(initialTimeout)
        {
            _taskCreator = taskCreator ?? throw new ArgumentNullException(nameof(taskCreator));
        }

        public FuncTimeoutWorker(int initialMillisecondsTimeout, Func<Task> taskCreator)
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
